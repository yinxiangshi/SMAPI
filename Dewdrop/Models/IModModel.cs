namespace Dewdrop.Models
{
    /// <summary>A mod metadata response which provides a method to extract generic info.</summary>
    internal interface IModModel
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get basic mod metadata.</summary>
        ModGenericModel ModInfo();
    }
}
