using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections;

namespace Wild_Card_Server
{
    class ServerMatchManager
    {
        public TempPlayer p1;
        public TempPlayer p2;

        public int matchID;

        public bool isActive = false;

        public MySqlConnection matchSQLConnection;


        public ServerMatchManager(int _matchID, TempPlayer _player1, TempPlayer _player2)
        {
            p1 = _player1;
            p2 = _player2;
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
            //Wait while both clients are ready
            while (!p1.Ready || !p2.Ready) { }
            isActive = true;
            SendResults();
            StartRound();
        }



        //TODO : REFACTOR IT!!
        public void StartRound()
        {
            DateTime roundStartTIme;
            //roundStartTIme = DateTime.Now.Add(TimeSpan.FromSeconds(Constants.LENGTH_OF_ROUND));
            while (isActive)
            {

                //Wait while client Catch Cards
                if ((p1.Ready && p2.Ready))
                {

                    SendCards();
                    p1.Ready = false;
                    p2.Ready = false;
                    roundStartTIme = DateTime.Now;
                    ServerTCP.PACKET_StartRound(p1.connectionID);
                    ServerTCP.PACKET_StartRound(p2.connectionID);

                    while ((!p1.Ready || !p2.Ready) && DateTime.Now.Subtract(roundStartTIme).Seconds <= Constants.LENGTH_OF_ROUND) { }

                    int p1SelectedCardID = p1.selectedCardID;
                    int p2SelectedCardID = p2.selectedCardID;
                    CalculateResults();
                    SendResults();
                    p1.SetDefaultValuesForResult();
                    p2.SetDefaultValuesForResult();

                }
                if (p1.health<=0 || p2.health <= 0)
                {
                    isActive = false;
                }

            }

            string winnerUsername = "Draw";
            if (p1.health<=0 && p2.health <= 0)
            {
                ServerTCP.PACKET_FinishGame(p1.connectionID, winnerUsername);
                ServerTCP.PACKET_FinishGame(p2.connectionID, winnerUsername);
            }
            else
            {
                winnerUsername = p1.health < p2.health ? p2.username : p1.username;
                ServerTCP.PACKET_FinishGame(p1.connectionID, winnerUsername);
                ServerTCP.PACKET_FinishGame(p2.connectionID, winnerUsername);
            }
        }

        private void CalculateResults()
        {


            TempPlayer[] players = { p1, p2 };
            foreach (var player in players)
            {
                if (player.selectedCardID != -1)
                {
                    string type = Constants.cards[player.selectedCardID].type;
                    switch (type)
                    {
                        case "Attack":
                            CalculateAttackCard(player);
                            break;
                        case "Heal":
                            CalculateHealCard(player);
                            break;
                        case "Item":
                            CalculateItemCard(player);
                            break;
                        case "Special":
                            CalculateSpecialCard(player);
                            break;
                    }
                }
                else
                {
                    CalculateNoCard(player);
                }
            }
            foreach (var player in players)
            {
                player.UpdateStats();

            }

        }

        private void CalculateNoCard(TempPlayer player)
        {
            player.UseEffects(false);
            player.UseEffects(true);

        }
        private void CalculateAttackCard(TempPlayer player)
        {


            AttackCard card = Constants.attackCards[player.selectedCardID];

            //Add initiative if player was first
            if (player.initiative && card.initiativeEffect != Constants.NoEffectID)
            {
                if (Constants.effects[card.initiativeEffect].selfEffect == 0)
                {
                    TempPlayer enemy = player == p1 ? p2 : p1;
                    enemy.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
                }
                else
                {
                    player.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
                }
            }






            //Save temp results for match
            player.results.dmgPerBullet = card.damage;
            player.results.bulletsSpent = card.bullets;
            player.results.accuracy += card.accuracy; //+ because default value for accuracy is 100

            //DETECT Other player for shooting
            TempPlayer other = player == p1 ? p2 : p1;

            switch (player.bodyPart)
            {
                case "Head":
                    player.AddEffect(Constants.ShootInHeadEffectID, 0, 0);
                    break;
                case "Arm":
                    player.AddEffect(Constants.ShootInArmEffectID, 0, 0);
                    break;
                case "Leg":
                    player.AddEffect(Constants.ShootInLegEffectID, 0, 0);
                    break;
                case "Body":
                    player.AddEffect(Constants.ShootInBodyEffectID, 0, 0);
                    break;
            }


            //Use PrefEffects
            player.UseEffects(true);


            

            player.MakeShots(other);

            //Use Post Effects
            player.UseEffects(false);

        }
        private void CalculateHealCard(TempPlayer player)
        {
            HealCard card = Constants.healCards[player.selectedCardID];

            if (player.initiative && card.initiativeEffect != Constants.NoEffectID)
            {
                if (Constants.effects[card.initiativeEffect].selfEffect == 0)
                {
                    TempPlayer enemy = player == p1 ? p2 : p1;
                    enemy.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
                }
                else
                {
                    player.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
                }
            }

            player.results.healing += card.heal;


            switch (player.bodyPart)
            {
                case "Head":
                    player.AddEffect(Constants.HealInHeadEffectID, 0, 0);
                    break;
                case "Arm":
                    player.AddEffect(Constants.HealInArmID, 0, 0);
                    break;
                case "Leg":
                    player.AddEffect(Constants.HealInLegID, 0, 0);
                    break;
                case "Body":
                    player.AddEffect(Constants.HealInBodyID, 0, 0);
                    break;
            }

            player.UseEffects(true);







            player.UseEffects(false);

        }
        private void CalculateItemCard(TempPlayer player)
        {
            ItemCard card = Constants.itemCards[player.selectedCardID];



            if (player.initiative && card.initiativeEffect != Constants.NoEffectID)
            {
                if (Constants.effects[card.initiativeEffect].selfEffect == 0)
                {
                    TempPlayer enemy = player == p1 ? p2 : p1;
                    enemy.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
                }
                else
                {
                    player.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
                }
            }


            //Use Pred Effects
            player.UseEffects(true);

            //No Actions here

            //Use Post Effects
            player.UseEffects(false);

        }
        //Temporary just make reload when choose reload card;
        private void CalculateSpecialCard(TempPlayer player)
        {
            player.UseEffects(true);
            player.n_bullets = player.max_bullets;
            player.UseEffects(false);
        }
        //TODO Rework Logic for random cards for EACH Player from DB
        private void SendCards()
        {

            //TODO CHANGE, NOW WORKS ONLY FOR ATTACK CARDS
            ArrayList cards = new ArrayList();

            Random rand = new Random();

            //Add Attack Card
            // TODO: GET BACK, DEBAGGING: 
            cards.Add(Constants.attackCards.ElementAt(rand.Next(0, Constants.attackCards.Count)).Key);
            //cards.Add(36);

            //Add Heal Card
            cards.Add(Constants.healCards.ElementAt(rand.Next(0, Constants.healCards.Count)).Key);

            //Add Item Card
            cards.Add(Constants.itemCards.ElementAt(rand.Next(0, Constants.itemCards.Count)).Key);


            ServerTCP.PACKET_SendCards(p1.connectionID, cards);
            ServerTCP.PACKET_SendCards(p2.connectionID, cards);

        }
        private void SendResults()
        {
            int p1SelectedCardID = p1.selectedCardID;
            int p2SelectedCardID = p2.selectedCardID;
            //Send Info to each player in format: playerHealth, EnemyHealth, PlayerBullets, EnemyBullets, PlayerCard, EnemyCard //Later add: PlayerAction, EnemyAction for animation
            //for player1:
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger(p1.Health);
            buffer.WriteInteger(p2.Health);
            buffer.WriteInteger(p1.Bullets);
            buffer.WriteInteger(p2.Bullets);
            buffer.WriteInteger(p1.results.accuracy);
            buffer.WriteInteger(p2.results.accuracy);


            //Send Active Effects
            //For Player
            buffer.WriteInteger(p1.effects.Count); //Write Number of Effects
            foreach (var eff in p1.effects.Keys)
            {
                buffer.WriteInteger(eff);
                buffer.WriteInteger(p1.effects[eff].Item1);//Value
                buffer.WriteInteger(p1.effects[eff].Item2); //Duration Time
            }
            //And for opponent
            buffer.WriteInteger(p2.effects.Count); //Write Number of Effects
            foreach (var eff in p2.effects.Keys)
            {
                buffer.WriteInteger(eff);
                buffer.WriteInteger(p2.effects[eff].Item1);//Value
                buffer.WriteInteger(p2.effects[eff].Item2); //Duration Time
            }

            buffer.WriteInteger(p1SelectedCardID);
            buffer.WriteInteger(p2SelectedCardID);

            //for player2:
            ByteBuffer buffer2 = new ByteBuffer();
            buffer2.WriteInteger(p2.Health);
            buffer2.WriteInteger(p1.Health);
            buffer2.WriteInteger(p2.Bullets);
            buffer2.WriteInteger(p1.Bullets);
            buffer2.WriteInteger(p2.results.accuracy);
            buffer2.WriteInteger(p1.results.accuracy);
            //For Player
            buffer2.WriteInteger(p2.effects.Count); //Write Number of Effects
            foreach (var eff in p2.effects.Keys)
            {
                buffer2.WriteInteger(eff);
                buffer2.WriteInteger(p2.effects[eff].Item1);//Value
                buffer2.WriteInteger(p2.effects[eff].Item2); //Duration Time
            }
            //For Opponent
            buffer2.WriteInteger(p1.effects.Count); //Write Number of Effects
            foreach (var eff in p1.effects.Keys)
            {
                buffer2.WriteInteger(eff);
                buffer2.WriteInteger(p1.effects[eff].Item1);//Value
                buffer2.WriteInteger(p1.effects[eff].Item2); //Duration Time
            }

            buffer2.WriteInteger(p2SelectedCardID);
            buffer2.WriteInteger(p1SelectedCardID);

            //Set Ready to false, for animations
            p1.Ready = false;
            p2.Ready = false;
            //deselect cards:


            //Sending:
            ServerTCP.PACKET_ShowResult(p1.connectionID, buffer.ToArray());
            ServerTCP.PACKET_ShowResult(p2.connectionID, buffer2.ToArray());
        }


        public void RestartMatch()
        {
            p1 = new TempPlayer(p1.connectionID, p1.username);
            p2 = new TempPlayer(p2.connectionID, p2.username);
            p1.Ready = false;
            p2.Ready = false;
            ServerTCP.PACKET_LoadMatch(p1.connectionID, matchID);
            ServerTCP.PACKET_LoadMatch(p2.connectionID, matchID);
            isActive = true;
        }

    }
}
