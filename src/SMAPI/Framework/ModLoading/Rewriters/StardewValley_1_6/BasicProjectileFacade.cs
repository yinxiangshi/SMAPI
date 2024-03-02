using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.Projectiles;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <see cref="BasicProjectile"/> methods to their newer form to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    public class BasicProjectileFacade : BasicProjectile, IRewriteFacade
    {
        /*********
        ** Public methods
        *********/
        public static BasicProjectile Constructor(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation? location = null, Character? firer = null, bool spriteFromObjectSheet = false, onCollisionBehavior? collisionBehavior = null)
        {
            var projectile = new BasicProjectile(
                damageToFarmer: damageToFarmer,
                spriteIndex: parentSheetIndex,
                bouncesTillDestruct: bouncesTillDestruct,
                tailLength: tailLength,
                rotationVelocity: rotationVelocity,
                xVelocity: xVelocity,
                yVelocity: yVelocity,
                startingPosition: startingPosition
            );

            projectile.explode.Value = explode;
            projectile.collisionSound.Value = collisionSound;
            projectile.damagesMonsters.Value = damagesMonsters;
            projectile.theOneWhoFiredMe.Set(location, firer);
            projectile.itemId.Value = spriteFromObjectSheet ? parentSheetIndex.ToString() : null;
            projectile.collisionBehavior = collisionBehavior;

            if (!string.IsNullOrWhiteSpace(firingSound) && location != null)
                location.playSound(firingSound);

            return projectile;
        }

        public static BasicProjectile Constructor(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition)
        {
            return Constructor(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, "flameSpellHit", "flameSpell", true);
        }


        /*********
        ** Private methods
        *********/
        private BasicProjectileFacade()
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
