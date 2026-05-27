using FluentValidation;
using Media.Domain.Constants;

namespace Media.Application.Commands.UploadMedia;

public class UploadMediaValidator : AbstractValidator<UploadMediaCommand>
{
    public UploadMediaValidator()
    {
        RuleFor(x => x.OriginalFileName).NotEmpty().MaximumLength(MediaConstants.MaxFileNameLength);
        RuleFor(x => x.ContentType).NotEmpty().Must(ct => MediaConstants.AllowedContentTypes.All.Contains(ct))
            .WithMessage($"Content type not allowed. Allowed: {string.Join(", ", MediaConstants.AllowedContentTypes.All)}");
        RuleFor(x => x.FileStream).NotNull();
        RuleFor(x => x.OwnedByUserId).NotEmpty();
    }
}
