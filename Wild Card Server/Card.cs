using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Experimental.UIElements;

namespace Wild_Card_Server
{
    class Card
    {
        public int ID; // ID in database
        public int Position;
        public bool Selected;
        public string Type;
        public string Name;
        public bool IsComboCard; // is it possible to get this card by combo
        public int NForCombo; //number of Cards that are needed if this card could be made from combo
        public List<int> ComboCards; //IDs of such Cards
        public string CardImage; //filename for client
        public string ItemImage; // filename for client 
        public int Value; // Value of card. Damage, heal, bonus
        public string Animation; // Name of the animation for client
        public int Direction; //0-self, 1 - enemy


        private Card()
        {

        }

        public Card(DBInstanceCard dbCard)
        {
            ID = dbCard.ID;
            Type = dbCard.Type;
            Name = dbCard.Name;
            IsComboCard = dbCard.IsComboCard;
            NForCombo = dbCard.NForCombo;
            ComboCards = new List<int>(dbCard.ComboCards);
            CardImage = dbCard.CardImage;
            ItemImage = dbCard.ItemImage;
            Value = dbCard.Value;
            Animation = dbCard.Animation;

        }

        public void UseCard(TempPlayer player)
        {
            switch (Type)
            {
                case "Attack":
                    player.GetDamage(Value);
                    break;
                case "Heal":
                    player.GetHeal(Value);
                    break;
                case "Armor":
                    player.GetArmor(Value);
                    break;
                case "Item":
                    player.GetDamage(Value);
                    break;
                default:
                    Console.WriteLine("Something Wrong. No type on card");
                    ;
                    break;
            }
        }

        public Card Clone()
        {
            var clone = new Card()
            {
                ID = this.ID,
                Position = this.Position,
                Selected = this.Selected,
                Type = this.Type,
                Name = this.Name,
                IsComboCard = this.IsComboCard,
                NForCombo = this.NForCombo,
                ComboCards = new List<int>(this.ComboCards),
                CardImage = this.CardImage,
                ItemImage = this.ItemImage,
                Value = this.Value,
                Animation = this.Animation,
                Direction = this.Direction
            };
            return clone;

        }
    }
}


