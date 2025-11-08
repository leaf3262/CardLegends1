using UnityEngine;

public enum PowerEffectType
{
    DrawExtra,
    ScoreMultiplier,
    DiscardBonus,
    HandSizeIncrease
}

public class PowerCardEffect
{
    public PowerEffectType effectType;
    public int value;
    public string description;

    public PowerCardEffect(PowerEffectType type, int val, string desc)
    {
        effectType = type;
        value = val;
        description = desc;
    }

    public void Execute()
    {
        switch (effectType)
        {
            case PowerEffectType.DrawExtra:
                ExecuteDrawExtra();
                break;
            case PowerEffectType.ScoreMultiplier:
                ExecuteScoreMultiplier();
                break;
            case PowerEffectType.DiscardBonus:
                ExecuteDiscardBonus();
                break;
            case PowerEffectType.HandSizeIncrease:
                ExecuteHandSizeIncrease();
                break;
        }
    }

    private void ExecuteDrawExtra()
    {
        Debug.Log($"Power Card: Draw {value} extra cards!");
        if (HandManager.Instance != null)
        {
            for (int i = 0; i < value; i++)
            {
                HandManager.Instance.DrawCard();
            }
        }
    }

    private void ExecuteScoreMultiplier()
    {
        Debug.Log($"Power Card: {value}x score multiplier activated!");
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.AddScoreMultiplier(value);
        }
    }

    private void ExecuteDiscardBonus()
    {
        Debug.Log($"Power Card: +{value} points for each discard!");
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.AddDiscardBonus(value);
        }
    }

    private void ExecuteHandSizeIncrease()
    {
        Debug.Log($"Power Card: Hand size increased by {value}!");
    }

    public static PowerCardEffect GetEffectFromCard(CardData cardData)
    {
        if (cardData.cardType != CardType.Power)
            return null;

        if (cardData.cardName.Contains("Draw"))
        {
            return new PowerCardEffect(PowerEffectType.DrawExtra, 2, "Draw 2 extra cards");
        }
        else if (cardData.cardName.Contains("Mult"))
        {
            return new PowerCardEffect(PowerEffectType.ScoreMultiplier, 2, "2x score multiplier");
        }
        else if (cardData.cardName.Contains("Discard"))
        {
            return new PowerCardEffect(PowerEffectType.DiscardBonus, 10, "+10 points per discard");
        }

        return null;
    }
}
