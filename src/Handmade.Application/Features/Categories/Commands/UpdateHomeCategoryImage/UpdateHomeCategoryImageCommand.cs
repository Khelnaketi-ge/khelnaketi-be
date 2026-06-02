using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Categories.Queries.GetHomeCategories;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UpdateHomeCategoryImage;

public sealed record UpdateHomeCategoryImageCommand(int HomeCategoryId, IFormFile Image)
    : IRequest<HomeCategoryDto>;

public sealed class UpdateHomeCategoryImageCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage) : IRequestHandler<UpdateHomeCategoryImageCommand, HomeCategoryDto>
{
    private const string HomeCategoryFolder = "home-categories";

    public async Task<HomeCategoryDto> Handle(
        UpdateHomeCategoryImageCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var homeCategory = await context.HomeCategories
            .Include(x => x.Category)
                .ThenInclude(x => x.Children)
            .Include(x => x.Image)
            .SingleOrDefaultAsync(x => x.Id == request.HomeCategoryId, cancellationToken);

        if (homeCategory is null)
        {
            throw new ValidationException(nameof(request.HomeCategoryId), "Home category was not found");
        }

        ImageUploadResult? uploadedImage = null;
        var oldObjectKey = homeCategory.Image?.ObjectKey;

        try
        {
            await using var imageStream = request.Image.OpenReadStream();
            uploadedImage = await imageStorage.UploadAsync(
                new ImageUploadRequest(
                    imageStream,
                    request.Image.FileName,
                    request.Image.ContentType,
                    request.Image.Length,
                    HomeCategoryFolder),
                cancellationToken);

            var imageAsset = new ImageAsset
            {
                Id = uploadedImage.Id,
                BucketName = uploadedImage.BucketName,
                ObjectKey = uploadedImage.ObjectKey,
                OriginalFileName = uploadedImage.OriginalFileName,
                ContentType = uploadedImage.ContentType,
                SizeBytes = uploadedImage.SizeBytes,
                UploadedByUserId = currentUser.Id.Value
            };

            context.ImageAssets.Add(imageAsset);
            homeCategory.ImageId = imageAsset.Id;
            homeCategory.Image = imageAsset;
            await context.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(oldObjectKey))
            {
                await imageStorage.DeleteAsync(oldObjectKey, CancellationToken.None);
            }

            return new HomeCategoryDto(
                homeCategory.Id,
                homeCategory.CategoryId,
                homeCategory.Category.Name,
                homeCategory.Category.Description,
                homeCategory.Category.ParentId,
                homeCategory.Category.Children.Count == 0,
                homeCategory.Order,
                imageAsset.Id,
                uploadedImage.PublicUrl);
        }
        catch
        {
            if (uploadedImage is not null)
            {
                await imageStorage.DeleteAsync(uploadedImage.ObjectKey, CancellationToken.None);
            }

            throw;
        }
    }
}
