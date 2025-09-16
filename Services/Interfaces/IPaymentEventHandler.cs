using System.Threading;
using System.Threading.Tasks;
using visita_booking_api.Services.Implementation;

namespace visita_booking_api.Services.Interfaces
{
    public interface IPaymentEventHandler
    {
        Task HandleAsync(PaymentEventDto paymentEvent, CancellationToken cancellationToken = default);
    }
}
