using MediatR;

namespace Afrodite.Commands
{
    public class RemoveFromCartCommand : IRequest
    {
        public int ProductId { get; set; }

        public RemoveFromCartCommand(int productId)
        {
            ProductId = productId;
        }
    }
}
