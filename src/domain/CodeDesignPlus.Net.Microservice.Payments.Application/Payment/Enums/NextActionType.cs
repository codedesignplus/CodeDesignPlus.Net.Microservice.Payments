namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Enums;


/// <summary>
/// Enum para el tipo de acción que el sistema debe tomar después de iniciar un pago.
/// </summary>
public enum NextActionType
{
    /// <summary>
    /// Redirigir al usuario a una URL específica proporcionada por el proveedor de pagos.
    /// </summary>
    Redirect,
    /// <summary>
    /// El frontend debe mostrar un widget de pago integrado proporcionado por el proveedor de pagos.
    /// </summary>
    DisplayWidget,
    /// <summary>
    /// Esperar una confirmación externa (por ejemplo, un webhook) antes de proceder.
    /// </summary>
    WaitConfirmation
}