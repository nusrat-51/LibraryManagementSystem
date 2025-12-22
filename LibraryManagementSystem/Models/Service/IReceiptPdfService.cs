using System.Threading.Tasks;

namespace LibraryManagementSystem.Services
{
    public interface IReceiptPdfService
    {
        Task<byte[]> BuildPaymentReceiptAsync(int paymentId);
    }
}
