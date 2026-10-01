namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

/// <summary>
/// La tarjeta que el usuario acaba de escribir, ya tokenizada por la pasarela. Es la única respuesta que lleva el
/// token: el navegador lo usa para pagar esa misma tarjeta y para guardarla. Una tarjeta ya guardada nunca lo devuelve;
/// para cobrarla se manda su id y ms-payments pone el token (pendings/180).
/// </summary>
public class TokenizedCardDto
{
    public Guid Id { get; set; }
    public string Token { get; set; } = null!;
    public string MaskedNumber { get; set; } = null!;
    public string Franchise { get; set; } = null!;
    public string CardHolderName { get; set; } = null!;
    public string ExpirationDate { get; set; } = null!;
    public string Last4Digits { get; set; } = null!;
}
