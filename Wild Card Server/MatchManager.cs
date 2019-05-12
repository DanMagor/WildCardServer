using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngineInternal;
using Random = System.Random;
using System.Net.Sockets;

namespace Wild_Card_Server
{
    class MatchManager
    {
        //Public Available Entities for data handling(ServerHandleData)
        public PlayerMatchEntity Player1;
        public PlayerMatchEntity Player2;
        public int MatchId;

        //States
        private bool isActive = false;
        private bool isCardChoosing = false;
        private bool restarRequested = false;

        //Connection to Database for specific Match
        private MySqlConnection matchSQLConnection;

        #region Initialization
        public MatchManager(int _matchID, PlayerMatchEntity _player1, PlayerMatchEntity _player2)
        {
            //Set entity values for match
            MatchId = _matchID;
            Player1 = _player1;
            Player2 = _player2;
            Player1.MatchID = MatchId;
            Player2.MatchID = MatchId;

            //Connect to Database
            matchSQLConnection = new MySqlConnection(MySQLConnector.CreateConnectionString());
            try
            {
                matchSQLConnection.Open();
                Console.WriteLine("Match №{0} Succesfully connected to MySQL Server '{1}'", MatchId, matchSQLConnection.ToString());
            }
            catch (Exception ex)
            {
                //TODO: Handle
                Console.WriteLine(ex.ToString());
                throw;
            }

        }
        private void InitializeMatch()
        {
            Player1.isReady = false;
            Player2.isReady = false;
            ServerTCP.PACKET_LoadMatch(Player1.ConnectionId, MatchId);
            ServerTCP.PACKET_LoadMatch(Player2.ConnectionId, MatchId);

        }
        #endregion

        public void StartMatch()
        {
            try
            {
                InitializeMatch();
                //Wait while both clients load Match Level
                while (!Player1.isReady || !Player2.isReady) { }
                isActive = true;
                FormDecks(); //form classic decks for players; For test: // FormTestDecks();
                PlayRound();
            }
            catch (SocketException e)
            {
                Console.WriteLine("Disconnect from player " + e.ErrorCode);
                var winner = Player1.ConnectionId == e.ErrorCode ? Player2 : Player1;
                ServerTCP.PACKET_Match_FinishGame(winner.ConnectionId, winner.Username);
            }
            Console.WriteLine("Match {0} is Finished", MatchId);
        }
        public void ToggleCardSelection(ByteBuffer data)
        {
            if (!isCardChoosing) return;

            var cardPos = data.ReadInteger();
            var connectionID = data.ReadInteger();

            var player = Player1.ConnectionId == connectionID ? Player1 : Player2;

            player.ToggleCardSelection(cardPos);

            ServerTCP.PACKET_Match_ConfirmToggleCard(player.ConnectionId, cardPos);
        }
        public void RequestRestart()
        {

            restarRequested = true;

        }
        public void RequestLeaveMatch(int leaverConnectionId)
        {
            isActive = false;
            var winner = Player1.ConnectionId == leaverConnectionId ? Player2 : Player1;
            var leaver = Player1.ConnectionId == leaverConnectionId ? Player1 : Player2;
            leaver.health = 0;
        }
        public void PlayerShot(int connectionID)
        {
            if (isCardChoosing)
            {
                isCardChoosing = false; //To prevent the same callback from other player during the execution
                var player = Player1.ConnectionId == connectionID ? Player1 : Player2;
                var other = Player1.ConnectionId == connectionID ? Player2 : Player1;
                player.Results.amIShot = true;
                other.Results.amIShot = false;
                player.isReady = true;
                other.isReady = true;
            }


        }


        private void PlayRound()
        {

            while (isActive && !restarRequested)
            {

                //Wait while client Catch Cards
                if ((Player1.isReady && Player2.isReady))
                {

                    SendCards(); //For Test: TEMP_SendComboCards();


                   
                    ServerTCP.PACKET_Match_StartRound(Player1.ConnectionId, Constants.LENGTH_OF_BEFORE_ROUND_TIMER);
                    ServerTCP.PACKET_Match_StartRound(Player2.ConnectionId, Constants.LENGTH_OF_BEFORE_ROUND_TIMER);
                    var timerStartTime = DateTime.Now;
                    while (DateTime.Now.Subtract(timerStartTime).Seconds <= Constants.LENGTH_OF_BEFORE_ROUND_TIMER) { }

                    isCardChoosing = true;

                    //Set not ready before card showing
                    Player1.isReady = false;
                    Player2.isReady = false;
                    ServerTCP.PACKET_Match_ShowCards(Player1.ConnectionId);
                    ServerTCP.PACKET_Match_ShowCards(Player2.ConnectionId);


                    var roundStartTime = DateTime.Now;
                    while ((!Player1.isReady || !Player2.isReady)
                        && DateTime.Now.Subtract(roundStartTime).Seconds <= Constants.LENGTH_OF_ROUND) { }

                    isCardChoosing = false;

                    CalculateResults();
                    

                    //Set Players State to not Ready and Wait When Result Animation Will Be finished
                    Player1.isReady = false;
                    Player2.isReady = false;
                    SendResults();

                    Player1.SetDefaultValuesForResult();
                    Player2.SetDefaultValuesForResult();

                }

                //Check if anyone is dead
                if (Player1.health <= 0 || Player2.health <= 0)
                {
                    isActive = false;
                }

            }
            //Check if restart was called. TODO: Delete Later, for Testing Purpose
            if (restarRequested)
            {
                restarRequested = false;
                RestartMatch();
            }
            else //If it's not restart, then someone is dead
            {
                var winnerUsername = "Draw";
                if (Player1.health <= 0 && Player2.health <= 0)
                {
                    ServerTCP.PACKET_Match_FinishGame(Player1.ConnectionId, winnerUsername);
                    ServerTCP.PACKET_Match_FinishGame(Player2.ConnectionId, winnerUsername);
                }
                else
                {
                    //Check who is alive
                    winnerUsername = Player1.health < Player2.health ? Player2.Username : Player1.Username;
                    ServerTCP.PACKET_Match_FinishGame(Player1.ConnectionId, winnerUsername);
                    ServerTCP.PACKET_Match_FinishGame(Player2.ConnectionId, winnerUsername);
                }
            }

            ////In case if match is finished, wait for restart request
            //while (!restarRequested) { }
            //restarRequested = false;
            //RestartMatch();

        }
        private void CalculateResults()
        {
            if (!Player1.Results.amIShot && !Player2.Results.amIShot)
            {
                var rand = new Random();
                var decisionNumber = rand.NextDouble();
                if (decisionNumber < 0.5)
                {
                    Player1.Results.amIShot = true;

                }
                else
                {
                    Player2.Results.amIShot = true;
                }
            }

            var playerOrderList = Player1.Results.amIShot ? new ArrayList { Player1, Player2 } : new ArrayList { Player2, Player1 };
            foreach (PlayerMatchEntity player in playerOrderList)
            {
                PlayerMatchEntity other = player.ConnectionId == Player1.ConnectionId ? Player2 : Player1;

                List<int> selfDirection = new List<int>();
                List<int> enemyDirection = new List<int>();

                //#TODO: Check that this is a godd fix no bugs here
                var copyCardsForRoundPos = new Dictionary<int, CardEntity>(player.cardsForRoundPos); //copy for avoiding problem with changed dictionary
                foreach (var card in copyCardsForRoundPos.Values)
                {

                    if (card.Selected)
                    {

                        if (card.Direction == 0)
                        {
                            selfDirection.Add(card.Position);
                        }
                        else
                        {
                            enemyDirection.Add(card.Position);
                        }
                    }
                    else
                    {

                        CardEntity temp = card.Clone();
                        temp.Direction = card.Direction == 0 ? 1 : 0;
                        player.CardDeck.Add(temp);
                        switch (temp.Type)
                        {
                            case "Attack":
                                player.Num_AttackCardsInDeck++;
                                break;
                            case "Heal":
                                player.Num_HealCardsInDeck++;
                                break;
                            case "Armor":
                                player.Num_ArmorCardsInDeck++;
                                break;
                            default:
                                throw new Exception("Wrong type of card in Calculate Result");
                        }

                    }
                }


                CheckCombos(player, selfDirection);
                CheckCombos(player, enemyDirection);

                //#TODO: Check that this is a godd fix no bugs here
                var copySoloCards = new List<int>(player.Results.soloCardsPos);
                foreach (var soloCards in copySoloCards)
                {

                    var card = player.cardsForRoundPos[soloCards];

                    other.Results.enemySelectedCards.Add(card.ID);
                    other.Results.enemySelectedCards.Add(card.Direction);

                    if (card.Direction == 0)
                    {
                        card.UseCard(player);
                    }
                    else
                    {
                        card.UseCard(other);
                    }
                    
                }
                //#TODO: Check that this is a godd fix no bugs here
                var copyComboCards = new List<List<int>>(player.Results.combos);
                foreach (var comboCards in copyComboCards)
                {
                    CardEntity tempCard = new CardEntity(Constants.Cards[comboCards[0]]);
                    if (comboCards[1] == 0)
                    {

                        tempCard.UseCard(player);
                    }
                    else
                    {
                        tempCard.UseCard(other);
                    }
                   
                    other.Results.enemySelectedCards.Add(comboCards[0]);
                    other.Results.enemySelectedCards.Add(comboCards[1]);

                    tempCard.Direction = 0;
                    player.CardDeck.Add(tempCard.Clone());

                    tempCard.Direction = 1;
                    player.CardDeck.Add(tempCard.Clone());

                    switch (tempCard.Type)
                    {
                        case "Attack":
                            player.Num_AttackCardsInDeck += 2;
                            break;
                        case "Heal":
                            player.Num_HealCardsInDeck += 2;
                            break;
                        case "Armor":
                            player.Num_ArmorCardsInDeck += 2;
                            break;
                        default:
                            throw new Exception("Wrong type of card in Calculate Result");
                    }
                }


            }

            foreach (PlayerMatchEntity player in playerOrderList)
            {
                var other = player.ConnectionId == Player1.ConnectionId ? Player2 : Player1;
                player.Results.playerHP = player.health;
                player.Results.playerArmor = player.armor;

                other.Results.enemyHP = player.health;
                other.Results.enemyArmor = player.armor;
            }


        }

        #region CombosChecking
        private void CheckCombos(PlayerMatchEntity player, List<int> cardPos)
        {
            switch (cardPos.Count)
            {
                case 4:
                    Check4CardCombo(player, cardPos, 4);
                    break;
                case 3:
                    Check3CardsCombo(player, cardPos, 3);
                    break;
                case 2:
                    Check2CardsCombo(player, cardPos, 2);
                    break;
                case 1:
                    player.Results.soloCardsPos.Add(cardPos[0]);
                    break;
                case 0:
                    break;
            }

        }

        private void Check4CardCombo(PlayerMatchEntity player, List<int> cardPos, int n)
        {
            List<int> temp = new List<int>();
            foreach (var card in cardPos)
            {
                temp.Add(player.cardsForRoundPos[card].ID);
            }

            temp.Sort();

            if (Constants.Combo4Cards.TryGetValue(temp, out var resultCard))
            {
                List<int> comboList = new List<int>();
                comboList.Add(resultCard.Id);
                comboList.Add(player.cardsForRoundPos[cardPos[0]].Direction);
                foreach (var card in cardPos)
                {
                    comboList.Add(card);
                }
                player.Results.combos.Add(comboList);

            }
            else
            {
                Check3CardsCombo(player, cardPos, n);
            }


        }

        private void Check3CardsCombo(PlayerMatchEntity player, List<int> cardPos, int n)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        if (i != j && j != k && i != k)
                        {
                            List<int> temp = new List<int>();
                            temp.Add(player.cardsForRoundPos[cardPos[i]].ID);
                            temp.Add(player.cardsForRoundPos[cardPos[j]].ID);
                            temp.Add(player.cardsForRoundPos[cardPos[k]].ID);
                            temp.Sort();

                            if (Constants.Combo3Cards.TryGetValue(temp, out var resultCard))
                            {
                                List<int> comboList = new List<int>();
                                comboList.Add(resultCard.Id);
                                comboList.Add(player.cardsForRoundPos[cardPos[0]].Direction);
                                comboList.Add(cardPos[i]);
                                comboList.Add(cardPos[j]);
                                comboList.Add(cardPos[k]);

                                player.Results.combos.Add(comboList);

                                if (n == 4)
                                {
                                    HashSet<int> full = new HashSet<int>() { 0, 1, 2, 3 };
                                    HashSet<int> cards3 = new HashSet<int>() { cardPos[i], cardPos[j], cardPos[k] };



                                    full.ExceptWith(cards3);

                                    player.Results.soloCardsPos.Add(full.Max());
                                }

                                return;

                            }

                        }
                    }
                }
            }

            Check2CardsCombo(player, cardPos, n);


        }

        private void Check2CardsCombo(PlayerMatchEntity player, List<int> cardPos, int n)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {

                        List<int> temp = new List<int>();
                        temp.Add(player.cardsForRoundPos[cardPos[i]].ID);
                        temp.Add(player.cardsForRoundPos[cardPos[j]].ID);

                        temp.Sort();

                        if (Constants.Combo2Cards.TryGetValue(temp, out var resultCard))
                        {
                            List<int> comboList = new List<int>();
                            comboList.Add(resultCard.Id);
                            comboList.Add(player.cardsForRoundPos[cardPos[0]].Direction);
                            comboList.Add(cardPos[i]);
                            comboList.Add(cardPos[j]);
                            player.Results.combos.Add(comboList);

                            // Check another 2 cards if we have it
                            if (cardPos.Count == 4)
                            {
                                HashSet<int> full = new HashSet<int>() { 0, 1, 2, 3 };
                                HashSet<int> cards2 = new HashSet<int>() { cardPos[i], cardPos[j] };
                                full.ExceptWith(cards2);
                                int firstPosition = full.Max();
                                int secondPosition = full.Min();
                                temp.Clear();
                                temp.Add(player.cardsForRoundPos[firstPosition].ID);
                                temp.Add(player.cardsForRoundPos[secondPosition].ID);
                                temp.Sort();

                                if (Constants.Combo2Cards.TryGetValue(temp, out var resultCard2))
                                {
                                    List<int> comboList2 = new List<int>();
                                    comboList2.Add(resultCard2.Id);
                                    comboList2.Add(player.cardsForRoundPos[cardPos[0]].Direction);
                                    comboList2.Add(firstPosition);
                                    comboList2.Add(secondPosition);
                                    player.Results.combos.Add(comboList2);

                                }
                                else
                                {
                                    player.Results.soloCardsPos.Add(firstPosition);
                                    player.Results.soloCardsPos.Add(secondPosition);
                                }
                            }
                            else if (cardPos.Count == 3)
                            {
                                HashSet<int> full = new HashSet<int>(cardPos);
                                HashSet<int> card2 = new HashSet<int>() { cardPos[i], cardPos[j] };
                                full.ExceptWith(card2);
                                player.Results.soloCardsPos.Add(full.Min());
                            }


                            return;

                        }
                    }
                }
            }



            foreach (var cPosition in cardPos)
            {
                player.Results.soloCardsPos.Add(cPosition);
            }

        }

        #endregion

  
        private void SendCards()
        {

            var rand = new Random();
            foreach (var player in new List<PlayerMatchEntity> { Player1, Player2 })
            {

                if (player.CardDeck.Count == 0)
                {
                    throw new Exception("Card Deck for " + player.Username + " is empty");
                }

                var cards = new ByteBuffer();
                var numberOfCards = Math.Min(4, player.CardDeck.Count);
                cards.WriteInteger(numberOfCards);

                for (var i = 0; i < numberOfCards; i++)
                {
                    int randIndex = rand.Next(0, player.CardDeck.Count);
                    var card = player.CardDeck.ElementAt(randIndex);
                    card.Selected = false;
                    card.Position = i;
                    player.cardsForRoundPos[i] = card;

                    player.CardDeck.RemoveAt(randIndex);

                    switch (card.Type)
                    {
                        case "Attack":
                            player.Num_AttackCardsInDeck--;
                            break;
                        case "Heal":
                            player.Num_HealCardsInDeck--;
                            break;
                        case "Armor":
                            player.Num_ArmorCardsInDeck--;
                            break;
                        default:
                            throw new Exception("Wrong type of card in SendCards");
                    }

                    cards.WriteInteger(card.ID);
                    cards.WriteInteger(card.Direction);

                }
                cards.WriteInteger(player.Num_AttackCardsInDeck);
                cards.WriteInteger(player.Num_HealCardsInDeck);
                cards.WriteInteger(player.Num_ArmorCardsInDeck);
                ServerTCP.PACKET_Match_SendCards(player.ConnectionId, cards);

            }
        }
        private void SendResults()
        {
            foreach (var player in new List<PlayerMatchEntity>() { Player1, Player2 })
            {
                var buffer = new ByteBuffer();

                buffer.WriteBool(player.Results.amIShot);
                buffer.WriteInteger(player.Results.soloCardsPos.Count);
                foreach (var resultsSoloCardsPo in player.Results.soloCardsPos)
                {
                    buffer.WriteInteger(resultsSoloCardsPo);
                }


                buffer.WriteInteger(player.Results.combos.Count);
                foreach (var resultsCombo in player.Results.combos)
                {
                    buffer.WriteInteger(resultsCombo.Count);
                    foreach (var insideResult in resultsCombo)
                    {
                        buffer.WriteInteger(insideResult);
                    }
                }

                List<int> notSelectedCards = new List<int>();
                foreach (var card in player.cardsForRoundPos.Values)
                {
                    if (card.Selected == false)
                    {
                        notSelectedCards.Add(card.Position);
                    }
                }
                buffer.WriteInteger(notSelectedCards.Count);
                foreach (var cardPos in notSelectedCards)
                {
                    buffer.WriteInteger(cardPos);
                }

              
                buffer.WriteInteger(player.Results.playerHP);
                buffer.WriteInteger(player.Results.playerArmor);

                buffer.WriteInteger(player.Results.enemySelectedCards.Count);
                foreach (var enemyCards in player.Results.enemySelectedCards)
                {
                    buffer.WriteInteger(enemyCards);
                }
                buffer.WriteInteger(player.Results.enemyHP);
                buffer.WriteInteger(player.Results.enemyArmor);


                buffer.WriteInteger(player.Num_AttackCardsInDeck);
                buffer.WriteInteger(player.Num_HealCardsInDeck);
                buffer.WriteInteger(player.Num_ArmorCardsInDeck);
                ////Sending:
                ServerTCP.PACKET_Match_ShowResult(player.ConnectionId, buffer.ToArray());
            }


        }
        //TODO: Optimize Later, remove redundancy loops
        private void FormDecks()
        {
            foreach (var player in new List<PlayerMatchEntity> { Player1, Player2 })
            {
                player.CardDeck = new List<CardEntity>();

                //Card IDS
                List<int> attackCardsIndexes = new List<int> { 1, 1, 5 };
                List<int> healCardsIndexes = new List<int> { 13, 14, 15 };
                List<int> armorCardsIndexes = new List<int> { 24, 25, 26 };

                //Repeat Multiple Times. Place N packs of Attack cards
                for (int i = 0; i < 4; i++)
                {
                    //Attack
                    foreach (var index in attackCardsIndexes)
                    {
                        var card = new CardEntity(Constants.Cards[index])
                        {
                            Direction = 0
                        };
                        player.CardDeck.Add(card);
                        player.Num_AttackCardsInDeck++;


                        card = card.Clone();
                        card.Direction = 1;
                        player.CardDeck.Add(card);
                        player.Num_AttackCardsInDeck++;
                    }
                }

                //Heal
                for (int i = 0; i < 2; i++)
                {
                    foreach (var index in healCardsIndexes)
                    {
                        var card = new CardEntity(Constants.Cards[index]);
                        card.Direction = 0;
                        player.CardDeck.Add(card);
                        player.Num_HealCardsInDeck++;

                        card = card.Clone();
                        card.Direction = 1;
                        player.CardDeck.Add(card);
                        player.Num_HealCardsInDeck++;
                    }
                }

                //Armor
                for (int i = 0; i < 2; i++)
                {
                    foreach (var index in armorCardsIndexes)
                    {
                        var card = new CardEntity(Constants.Cards[index]);
                        card.Direction = 0;
                        player.CardDeck.Add(card);
                        player.Num_ArmorCardsInDeck++;

                        card = card.Clone();
                        card.Direction = 1;
                        player.CardDeck.Add(card);
                        player.Num_ArmorCardsInDeck++;
                    }
                }
            }
        }

        private void RestartMatch()
        {
            isActive = false;
            //Recreate Players Entities
            Player1 = new PlayerMatchEntity(Player1.ConnectionId, Player1.Username);
            Player2 = new PlayerMatchEntity(Player2.ConnectionId, Player2.Username);
            StartMatch();
        }

        
        #region For Testing
        private void TEMP_SendComboCards()
        {
            var cards = new ByteBuffer();
            cards.WriteInteger(4);
            var card = new CardEntity(Constants.Cards[1]);

            card.Selected = false;
            card.Direction = 1;
            cards.WriteInteger(card.ID);
            cards.WriteInteger(card.Direction);
            card.Position = 0;
            Player1.cardsForRoundPos[0] = card;
            Player2.cardsForRoundPos[0] = card.Clone();

            card = new CardEntity(Constants.Cards[1]);
            card.Selected = false;
            card.Direction = 1;
            cards.WriteInteger(card.ID);
            cards.WriteInteger(card.Direction);
            card.Position = 1;
            Player1.cardsForRoundPos[1] = card;
            Player2.cardsForRoundPos[1] = card.Clone();

            card = new CardEntity(Constants.Cards[5]);
            card.Selected = false;
            card.Direction = 1;
            cards.WriteInteger(card.ID);
            cards.WriteInteger(card.Direction);
            card.Position = 2;
            Player1.cardsForRoundPos[2] = card;
            Player2.cardsForRoundPos[2] = card.Clone();

            card = new CardEntity(Constants.Cards[5]);
            card.Selected = false;
            card.Direction = 1;
            cards.WriteInteger(card.ID);
            cards.WriteInteger(card.Direction);
            card.Position = 3;
            Player1.cardsForRoundPos[3] = card;
            Player2.cardsForRoundPos[3] = card.Clone();


            ServerTCP.PACKET_Match_SendCards(Player1.ConnectionId, cards);
            ServerTCP.PACKET_Match_SendCards(Player2.ConnectionId, cards);

        }
        private void FormTestDecks()
        {
            foreach (var player in new List<PlayerMatchEntity> { Player1, Player2 })
            {
                player.CardDeck = new List<CardEntity>();

                List<int> attackCardsIndexes = new List<int> { 1, 1, 1 };
                //List<int> healCardsIndexes = new List<int> { 13, 14, 15 };
                //List<int> armorCardsIndexes = new List<int> { 24, 25, 26 };

                for (int i = 0; i < 4; i++)
                {
                    foreach (var index in attackCardsIndexes)
                    {
                        var card = new CardEntity(Constants.Cards[index]);

                        //if (i % 2 == 0)
                        //{
                        //    card.Direction = 0;
                        //    player.cardDeck.Add(card);
                        //}

                        //card = card.Clone();
                        card.Direction = 0;
                        player.CardDeck.Add(card);
                    }
                }
                //for (int i = 0; i < 2; i++)
                //{
                //    foreach (var index in healCardsIndexes)
                //    {
                //        var card = new Card(Constants.Cards[index]);
                //        card.Direction = 0;
                //        player.cardDeck.Add(card);

                //        card = card.Clone();
                //        card.Direction = 1;
                //        player.cardDeck.Add(card);
                //    }
                //}

                //for (int i = 0; i < 2; i++)
                //{
                //    foreach (var index in armorCardsIndexes)
                //    {
                //        var card = new Card(Constants.Cards[index]);
                //        card.Direction = 0;
                //        player.cardDeck.Add(card);

                //        card = card.Clone();
                //        card.Direction = 1;
                //        player.cardDeck.Add(card);
                //    }
                //}
            }
        }
        #endregion

    }
}
