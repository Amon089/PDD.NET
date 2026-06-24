using Habitto.Domain.Entities;
using Habitto.Domain.Interfaces;
using MediatR;

namespace Habitto.Application.Wishlist;

public sealed record AddToWishlistCommand(Guid UserId, Guid PropertyId) : IRequest;

public sealed class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand>
{
    private readonly IWishlistRepository _wishlist;
    private readonly IUnitOfWork _unitOfWork;

    public AddToWishlistCommandHandler(IWishlistRepository wishlist, IUnitOfWork unitOfWork)
    {
        _wishlist = wishlist;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddToWishlistCommand request, CancellationToken ct)
    {
        await _wishlist.AddAsync(new WishlistItem(request.UserId, request.PropertyId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

public sealed record RemoveFromWishlistCommand(Guid UserId, Guid PropertyId) : IRequest;

public sealed class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand>
{
    private readonly IWishlistRepository _wishlist;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromWishlistCommandHandler(IWishlistRepository wishlist, IUnitOfWork unitOfWork)
    {
        _wishlist = wishlist;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveFromWishlistCommand request, CancellationToken ct)
    {
        await _wishlist.RemoveAsync(request.UserId, request.PropertyId, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

public sealed record GetWishlistQuery(Guid UserId) : IRequest<IReadOnlyList<WishlistItem>>;

public sealed class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, IReadOnlyList<WishlistItem>>
{
    private readonly IWishlistRepository _wishlist;

    public GetWishlistQueryHandler(IWishlistRepository wishlist) => _wishlist = wishlist;

    public Task<IReadOnlyList<WishlistItem>> Handle(GetWishlistQuery request, CancellationToken ct)
        => _wishlist.GetByUserAsync(request.UserId, ct);
}
