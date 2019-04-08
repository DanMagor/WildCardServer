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

namespace Wild_Card_Server
{
    class ServerMatchManager
    {
        public TempPlayer p1;
        public TempPlayer p2;

        public int matchID;

        public bool isActive = false;
        private bool isCardChoosing = false;

        public MySqlConnection matchSQLConnection;


        public ServerMatchManager(int _matchID, TempPlayer _player1, TempPlayer _player2)
        {
            p1 = _player1;
            p2 = _player2;
            p1.Ready = false;
            p2.Ready = false;
            matchID = _matchID;

            p1.matchID = matchID;
            p2.matchID = matchID;
            matchSQLConnection = new MySqlConnection(MySQL.CreateConnectionString());
            try
            {
                matchSQLConnection.Open();
                Console.WriteLine("Match №{0} Succesfully connected to MySQL Server '{1}'", matchID, matchSQLConnection.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

        public void InitializeMatch()
        {
            ServerTCP.PACKET_LoadMatch(p1.connectionID, matchID);
            ServerTCP.PACKET_LoadMatch(p2.connectionID, matchID);
           
        }

        public void StartMatch()
        {
            InitializeMatch();
            //Wait while both clients are ready
            while (!p1.Ready || !p2.Ready) { }
            isActive = true;
          //  SendResults();
            StartRound();
        }



        //TODO : REFACTOR IT!!
        public void StartRound()
        {   
            while (isActive)
            {

                //Wait while client Catch Cards
                if ((p1.Ready && p2.Ready))
                {

                    SendCards();
                    //TEMP_SendComboCards();

                    p1.Ready = false;
                    p2.Ready = false;
                    
                    var timerTime = 1.0f;
                    ServerTCP.PACKET_StartRound(p1.connectionID,timerTime);
                    ServerTCP.PACKET_StartRound(p2.connectionID, timerTime);
                    var timerStartTime = DateTime.Now;
                    while (DateTime.Now.Subtract(timerStartTime).Seconds <= timerTime) { }
                    
                    isCardChoosing = true;
                    ServerTCP.PACKET_Match_ShowCards(p1.connectionID);
                    ServerTCP.PACKET_Match_ShowCards(p2.connectionID);
                    var roundStartTIme = DateTime.Now;
                    while ((!p1.Ready || !p2.Ready) && DateTime.Now.Subtract(roundStartTIme).Seconds <= Constants.LENGTH_OF_ROUND) { }

                    isCardChoosing = false;
                    
                    CalculateResults();
                    p1.Ready = false;
                    p2.Ready = false;
                    SendResults();
                    p1.SetDefaultValuesForResult();
                    p2.SetDefaultValuesForResult();

                }
                if (p1.Health<=0 || p2.Health <= 0)
                {
                    isActive = false;
                }

            }

            var winnerUsername = "Draw";
            if (p1.Health<=0 && p2.Health <= 0)
            {
                ServerTCP.PACKET_FinishGame(p1.connectionID, winnerUsername);
                ServerTCP.PACKET_FinishGame(p2.connectionID, winnerUsername);
            }
            else
            {
                winnerUsername = p1.Health < p2.Health ? p2.username : p1.username;
                ServerTCP.PACKET_FinishGame(p1.connectionID, winnerUsername);
                ServerTCP.PACKET_FinishGame(p2.connectionID, winnerUsername);
            }
            
        }

        private void CalculateResults()
        {
            if (!p1.results.amIShot && !p2.results.amIShot)
            {
                var rand = new Random();
                var decisionNumber = rand.NextDouble();
                if (decisionNumber < 0.5)
                {
                    p1.results.amIShot = true;

                }
                else
                {
                    p2.results.amIShot = true;
                }
            }

            var playerOrderList = p1.results.amIShot ? new ArrayList {p1, p2} : new ArrayList {p2, p1};
            foreach (TempPlayer player in playerOrderList)
            {
                TempPlayer other = player.connectionID == p1.connectionID ? p2 : p1;

                List<int> selfDirection = new List<int>();
                List<int> enemyDirection = new List<int>();

                //#TODO: Check that this is a godd fix no bugs here
                var copyCardsForRoundPos = new Dictionary<int,Card>(player.cardsForRoundPos); //copy for avoiding problem with changed dictionary
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
                }
                

                CheckCombos(player, selfDirection);
                CheckCombos(player, enemyDirection);

                //#TODO: Check that this is a godd fix no bugs here
                var copySoloCards = new List<int>(player.results.soloCardsPos);
                foreach (var soloCards in copySoloCards)
                {

                    var card = player.cardsForRoundPos[soloCards];

                    other.results.enemySelectedCards.Add(card.ID);
                    other.results.enemySelectedCards.Add(card.Direction);

                    if (card.Direction == 0)
                    {
                        card.UseCard(player);
                    }
                    else
                    {
                        card.UseCard(other);
                    }
                    Console.WriteLine("Card {0} was used by {1}", card.ID, player.username);
                }
                //#TODO: Check that this is a godd fix no bugs here
                var copyComboCards = new List<List<int>>(player.results.combos);
                foreach (var comboCards in copyComboCards)
                {
                    Card tempCard = new Card(Constants.Cards[comboCards[0]]);
                    if (comboCards[1] == 0)
                    {
                        
                        tempCard.UseCard(player);
                    }
                    else
                    {
                        tempCard.UseCard(other);
                    }
                    Console.WriteLine("Card {0} was used by {1}", tempCard.ID, player.username);
                    other.results.enemySelectedCards.Add(comboCards[0]);
                    other.results.enemySelectedCards.Add(comboCards[1]);

                }

               
            }

            foreach (TempPlayer player in playerOrderList)
            {
                var other = player.connectionID == p1.connectionID ? p2 : p1;
                player.results.playerHP = player.Health;
                player.results.playerArmor = player.Armor;

                other.results.enemyHP = player.Health;
                other.results.enemyArmor = player.Armor;
            }
               

        }


        #region CombosChecking
        private void CheckCombos(TempPlayer player, List<int> cardPos)
        {
            switch (cardPos.Count)
            {
                case 4:
                    Check4CardCombo(player, cardPos ,4);
                    break;
                case 3:
                    Check3CardsCombo(player, cardPos, 3);
                    break;
                case 2:
                    Check2CardsCombo(player, cardPos, 2);
                    break;
                case 1:
                    player.results.soloCardsPos.Add(cardPos[0]);
                    break;
                case 0:
                    break;
            }
           
        }

        private void Check4CardCombo(TempPlayer player, List<int> cardPos, int n)
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
                comboList.Add(resultCard.ID);
                comboList.Add(player.cardsForRoundPos[cardPos[0]].Direction);
                foreach (var card in cardPos)
                {
                    comboList.Add(card);
                }

            }
            else
            {
                Check3CardsCombo(player, cardPos, n);
            }
            
            
        }

        private void Check3CardsCombo(TempPlayer player, List<int> cardPos, int n)
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
                                comboList.Add(resultCard.ID);
                                comboList.Add(player.cardsForRoundPos[cardPos[0]].Direction);
                                comboList.Add(cardPos[i]);
                                comboList.Add(cardPos[j]);
                                comboList.Add(cardPos[k]);

                                player.results.combos.Add(comboList);

                                HashSet<int> full = new HashSet<int>(){0,1,2,3};
                                HashSet<int> cards3 = new HashSet<int>(){ cardPos[i], cardPos[j], cardPos[k] };



                                full.ExceptWith(cards3);
                                
                                player.results.soloCardsPos.Add(full.Max());
                                
                                return;

                            }

                        }
                    }
                }
            }

            Check2CardsCombo(player, cardPos, n);


        }

        private void Check2CardsCombo(TempPlayer player, List<int> cardPos, int n)
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
                            comboList.Add(resultCard.ID);
                            comboList.Add(player.cardsForRoundPos[cardPos[0]].Direction);
                            comboList.Add(cardPos[i]);
                            comboList.Add(cardPos[j]);
                            player.results.combos.Add(comboList);

                            // Check another 2 cards if we have it
                            if (cardPos.Count == 4)
                            {
                                HashSet<int> full = new HashSet<int>() {0, 1, 2, 3};
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
                                    comboList2.Add(resultCard2.ID);
                                    comboList2.Add(player.cardsForRoundPos[cardPos[0]].Direction);
                                    comboList2.Add(firstPosition);
                                    comboList2.Add(secondPosition);
                                    player.results.combos.Add(comboList2);

                                }
                                else
                                {
                                    player.results.soloCardsPos.Add(firstPosition);
                                    player.results.soloCardsPos.Add(secondPosition);
                                }
                            }
                            else if (cardPos.Count == 3)
                            {
                                HashSet<int> full = new HashSet<int>(cardPos);
                                HashSet<int> card2 = new HashSet<int>(){ cardPos[i], cardPos[j] };
                                full.ExceptWith(card2);
                                player.results.soloCardsPos.Add(full.Min());
                            }


                            return;

                        }
                    }
                }
            }


           
            foreach (var cPosition in cardPos)
            {
                player.results.soloCardsPos.Add(cPosition);
            }

        }

        #endregion





        public void ToggleCardSelection(ByteBuffer data)
        {
            if (!isCardChoosing) return;

            var cardPos = data.ReadInteger();
            var connectionID = data.ReadInteger();

            var player = p1.connectionID == connectionID ? p1 : p2;

            player.ToggleCardSelection(cardPos);

            ServerTCP.PACKET_ConfirmToggleCard(player.connectionID);
        }

        private void TEMP_SendComboCards()
        {
            var cards = new ByteBuffer();
            cards.WriteInteger(4);
            var card = new Card(Constants.Cards[1]);
            
            card.Selected = false;
            card.Direction = 1;
            cards.WriteInteger(card.ID);
            cards.WriteInteger(card.Direction);
            card.Position = 0;
            p1.cardsForRoundPos[0] = card;
            p2.cardsForRoundPos[0] = card.Clone();

            card = new Card(Constants.Cards[1]);
            card.Selected = false;
            card.Direction = 1;
            cards.WriteInteger(card.ID);
            cards.WriteInteger(card.Direction);
            card.Position = 1;
            p1.cardsForRoundPos[1] = card;
            p2.cardsForRoundPos[1] = card.Clone();

            card = new Card(Constants.Cards[5]);
            card.Selected = false;
            card.Direction = 1;
            cards.WriteInteger(card.ID);
            cards.WriteInteger(card.Direction);
            card.Position = 2;
            p1.cardsForRoundPos[2] = card;
            p2.cardsForRoundPos[2] = card.Clone();

            card = new Card(Constants.Cards[5]);
            card.Selected = false;
            card.Direction = 1;
            cards.WriteInteger(card.ID);
            cards.WriteInteger(card.Direction);
            card.Position = 3;
            p1.cardsForRoundPos[3] = card;
            p2.cardsForRoundPos[3] = card.Clone();


            ServerTCP.PACKET_SendCards(p1.connectionID, cards);
            ServerTCP.PACKET_SendCards(p2.connectionID, cards);

        }
        private void SendCards()
        {

            
            //TODO: Add logic for Deck and managing probabilities

            var cards = new ByteBuffer();

            var rand = new Random();

            var numberOfCards = 4;
            cards.WriteInteger(numberOfCards);

            for (var i = 0; i < numberOfCards; i++)
            {
                var card = new Card(Constants.Cards.ElementAt(rand.Next(0, Constants.Cards.Count)).Value)
                {
                    Selected = false, Direction = rand.Next(0, 2), Position = i
                };


                
                p1.cardsForRoundPos[i] = card;
               

                p2.cardsForRoundPos[i] = card.Clone();

                cards.WriteInteger(card.ID);
                cards.WriteInteger(card.Direction);

                
            }
            
            ServerTCP.PACKET_SendCards(p1.connectionID, cards);
            ServerTCP.PACKET_SendCards(p2.connectionID, cards);

        }
        private void SendResults()
        {
            foreach (var player in new List<TempPlayer>(){p1,p2})
            {
                var buffer = new ByteBuffer();

                buffer.WriteBool(player.results.amIShot);
                buffer.WriteInteger(player.results.soloCardsPos.Count);
                foreach (var resultsSoloCardsPo in player.results.soloCardsPos)
                {
                    buffer.WriteInteger(resultsSoloCardsPo);
                }


                buffer.WriteInteger(player.results.combos.Count);
                foreach (var resultsCombo in player.results.combos)
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

                Console.WriteLine("Player {0} has {1} HP and {2} armor", player.username, player.results.playerHP, player.results.playerArmor );
                buffer.WriteInteger(player.results.playerHP);
                buffer.WriteInteger(player.results.playerArmor);

                buffer.WriteInteger(player.results.enemySelectedCards.Count);
                foreach (var enemyCards in player.results.enemySelectedCards)
                {
                    buffer.WriteInteger(enemyCards);
                }
                buffer.WriteInteger(player.results.enemyHP);
                buffer.WriteInteger(player.results.enemyArmor);

                ////Sending:
                ServerTCP.PACKET_ShowResult(player.connectionID, buffer.ToArray());
            }
            
            
        }


        public void RestartMatch()
        {
            p1 = new TempPlayer(p1.connectionID, p1.username);
            p2 = new TempPlayer(p2.connectionID, p2.username);
            p1.Ready = false;
            p2.Ready = false;
            StartMatch();
            
           
        }

        public void PlayerShot(int connectionID)
        {
            if (isCardChoosing)
            {
                var player = p1.connectionID == connectionID ? p1 : p2;
                var other = p1.connectionID == connectionID ? p2 : p1;
                player.results.amIShot = true;
                other.results.amIShot = false;
                player.Ready = true;
                other.Ready = true;

            }


        }

    }
}
