using Unity.Netcode;
using UnityEngine;
using System;

public struct NetworkCardData : INetworkSerializable, IEquatable<NetworkCardData>
{
    public int cardIndex;
    public CardType cardType;
    public CardSuit suit;
    public CardRank rank;

    public NetworkCardData(CardData cardData, int index)
    {
        cardIndex = index;
        cardType = cardData.cardType;
        suit = cardData.suit;
        rank = cardData.rank;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref cardIndex);

        if (serializer.IsWriter)
        {
            int typeInt = (int)cardType;
            int suitInt = (int)suit;
            int rankInt = (int)rank;
            serializer.SerializeValue(ref typeInt);
            serializer.SerializeValue(ref suitInt);
            serializer.SerializeValue(ref rankInt);
        }
        else
        {
            int typeInt = 0, suitInt = 0, rankInt = 0;
            serializer.SerializeValue(ref typeInt);
            serializer.SerializeValue(ref suitInt);
            serializer.SerializeValue(ref rankInt);
            cardType = (CardType)typeInt;
            suit = (CardSuit)suitInt;
            rank = (CardRank)rankInt;
        }
    }

    public bool Equals(NetworkCardData other)
    {
        return cardIndex == other.cardIndex &&
               cardType == other.cardType &&
               suit == other.suit &&
               rank == other.rank;
    }

    public override bool Equals(object obj)
    {
        return obj is NetworkCardData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(cardIndex, cardType, suit, rank);
    }

    public static bool operator ==(NetworkCardData left, NetworkCardData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NetworkCardData left, NetworkCardData right)
    {
        return !left.Equals(right);
    }
}
