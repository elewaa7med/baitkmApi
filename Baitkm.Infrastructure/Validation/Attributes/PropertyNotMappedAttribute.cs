using System.ComponentModel.DataAnnotations;

namespace Baitkm.Infrastructure.Validation.Attributes
{
    /// <summary>
    /// This attribute used for track properties that doesn't need to be tracked
    /// </summary>
    public class PropertyNotMappedAttribute : ValidationAttribute
    {
    }
}
