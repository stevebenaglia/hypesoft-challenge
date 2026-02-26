namespace Hypesoft.Domain.Services;

/// <summary>
/// Abstracts unique identifier generation so Domain has no dependency on any specific ID library.
/// </summary>
public interface IIdGenerator
{
    string NewId();
}
