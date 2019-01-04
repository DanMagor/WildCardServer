public enum ServerPackages
{
    SWelcomeMsg = 1,
    SAlertMsg,
    SLoadMenu,
    SLoadMatch,
    SSendCards,

}

public enum ClientPackages
{
    CLogin = 1,
    CThankYou,
    CNewAccount,
    CSearchOpponent,
    CReadyForFight,
}



public class Card
{
    public enum CardTypes
    {
        Attack,
        Heal,
        Item,
    };

    public Card(string _name, CardTypes _cardType, int _damage)
    {
        name = _name;
        cardType = _cardType;
        damage = _damage;
    }

    public string name;
    public CardTypes cardType;
    public int damage;

}