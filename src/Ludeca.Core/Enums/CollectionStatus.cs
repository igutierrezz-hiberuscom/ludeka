namespace Ludeca.Core.Enums;

public enum CollectionStatus
{
    InCollection = 1, // 🟢 En mi ludoteca (físico propio)
    Played = 2,       // 🔵 Jugado (asociación, bar, amigos)
    Wishlist = 3,     // 🟡 Deseado (radar de interés)
    WantToBuy = 4     // 🔴 Quiero comprar (lista de seguimiento)
}
