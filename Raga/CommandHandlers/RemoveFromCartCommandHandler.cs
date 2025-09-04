using MediatR;
using Afrodite.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Afrodite.CommandHandlers
{
    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand>
    {
        private readonly ICartRepository _cartRepository;

        public RemoveFromCartCommandHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public Task<Unit> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            var cart = _cartRepository.GetCart();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == request.ProductId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                _cartRepository.SaveCart(cart);
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
