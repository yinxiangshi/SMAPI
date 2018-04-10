using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StardewModdingAPI.ModBuildConfig.Analyzer
{
    /// <summary>Detects implicit conversion from Stardew Valley's <c>Netcode</c> types. These have very unintuitive implicit conversion rules, so mod authors should always explicitly convert the type with appropriate null checks.</summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NetFieldAnalyzer : DiagnosticAnalyzer
    {
        /*********
        ** Properties
        *********/
        /// <summary>The namespace for Stardew Valley's <c>Netcode</c> types.</summary>
        private const string NetcodeNamespace = "Netcode";

        /// <summary>Maps net fields to their equivalent non-net properties where available.</summary>
        private readonly IDictionary<string, string> NetFieldWrapperProperties = new Dictionary<string, string>
        {
            // Character
            ["StardewValley.Character::currentLocationRef"] = "currentLocation",
            ["StardewValley.Character::facingDirection"] = "FacingDirection",
            ["StardewValley.Character::name"] = "Name",
            ["StardewValley.Character::position"] = "Position",
            ["StardewValley.Character::scale"] = "Scale",
            ["StardewValley.Character::speed"] = "Speed",
            ["StardewValley.Character::sprite"] = "Sprite",

            // Chest
            ["StardewValley.Objects.Chest::tint"] = "Tint",

            // Farmer
            ["StardewValley.Farmer::houseUpgradeLevel"] = "HouseUpgradeLevel",
            ["StardewValley.Farmer::isMale"] = "IsMale",
            ["StardewValley.Farmer::items"] = "Items",
            ["StardewValley.Farmer::magneticRadius"] = "MagneticRadius",
            ["StardewValley.Farmer::stamina"] = "Stamina",
            ["StardewValley.Farmer::uniqueMultiplayerID"] = "UniqueMultiplayerID",
            ["StardewValley.Farmer::usingTool"] = "UsingTool",

            // Forest
            ["StardewValley.Locations.Forest::netTravelingMerchantDay"] = "travelingMerchantDay",
            ["StardewValley.Locations.Forest::netLog"] = "log",

            // FruitTree
            ["StardewValley.TerrainFeatures.FruitTree::greenHouseTileTree"] = "GreenHouseTileTree",
            ["StardewValley.TerrainFeatures.FruitTree::greenHouseTree"] = "GreenHouseTree",

            // GameLocation
            ["StardewValley.GameLocation::isFarm"] = "IsFarm",
            ["StardewValley.GameLocation::isOutdoors"] = "IsOutdoors",
            ["StardewValley.GameLocation::lightLevel"] = "LightLevel",
            ["StardewValley.GameLocation::name"] = "Name",

            // Item
            ["StardewValley.Item::category"] = "Category",
            ["StardewValley.Item::netName"] = "Name",
            ["StardewValley.Item::parentSheetIndex"] = "ParentSheetIndex",
            ["StardewValley.Item::specialVariable"] = "SpecialVariable",

            // Junimo
            ["StardewValley.Characters.Junimo::eventActor"] = "EventActor",

            // LightSource
            ["StardewValley.LightSource::identifier"] = "Identifier",

            // Monster
            ["StardewValley.Monsters.Monster::damageToFarmer"] = "DamageToFarmer",
            ["StardewValley.Monsters.Monster::experienceGained"] = "ExperienceGained",
            ["StardewValley.Monsters.Monster::health"] = "Health",
            ["StardewValley.Monsters.Monster::maxHealth"] = "MaxHealth",
            ["StardewValley.Monsters.Monster::netFocusedOnFarmers"] = "focusedOnFarmers",
            ["StardewValley.Monsters.Monster::netWildernessFarmMonster"] = "wildernessFarmMonster",
            ["StardewValley.Monsters.Monster::slipperiness"] = "Slipperiness",

            // NPC
            ["StardewValley.NPC::age"] = "Age",
            ["StardewValley.NPC::birthday_Day"] = "Birthday_Day",
            ["StardewValley.NPC::birthday_Season"] = "Birthday_Season",
            ["StardewValley.NPC::breather"] = "Breather",
            ["StardewValley.NPC::defaultMap"] = "DefaultMap",
            ["StardewValley.NPC::gender"] = "Gender",
            ["StardewValley.NPC::hideShadow"] = "HideShadow",
            ["StardewValley.NPC::isInvisible"] = "IsInvisible",
            ["StardewValley.NPC::isWalkingTowardPlayer"] = "IsWalkingTowardPlayer",
            ["StardewValley.NPC::manners"] = "Manners",
            ["StardewValley.NPC::optimism"] = "Optimism",
            ["StardewValley.NPC::socialAnxiety"] = "SocialAnxiety",

            // Object
            ["StardewValley.Object::canBeGrabbed"] = "CanBeGrabbed",
            ["StardewValley.Object::canBeSetDown"] = "CanBeSetDown",
            ["StardewValley.Object::edibility"] = "Edibility",
            ["StardewValley.Object::flipped"] = "Flipped",
            ["StardewValley.Object::fragility"] = "Fragility",
            ["StardewValley.Object::hasBeenPickedUpByFarmer"] = "HasBeenPickedUpByFarmer",
            ["StardewValley.Object::isHoedirt"] = "IsHoeDirt",
            ["StardewValley.Object::isOn"] = "IsOn",
            ["StardewValley.Object::isRecipe"] = "IsRecipe",
            ["StardewValley.Object::isSpawnedObject"] = "IsSpawnedObject",
            ["StardewValley.Object::minutesUntilReady"] = "MinutesUntilReady",
            ["StardewValley.Object::netName"] = "name",
            ["StardewValley.Object::price"] = "Price",
            ["StardewValley.Object::quality"] = "Quality",
            ["StardewValley.Object::scale"] = "Scale",
            ["StardewValley.Object::stack"] = "Stack",
            ["StardewValley.Object::tileLocation"] = "TileLocation",
            ["StardewValley.Object::type"] = "Type",

            // Projectile
            ["StardewValley.Projectiles.Projectile::ignoreLocationCollision"] = "IgnoreLocationCollision",

            // Tool
            ["StardewValley.Tool::currentParentTileIndex"] = "CurrentParentTileIndex",
            ["StardewValley.Tool::indexOfMenuItemView"] = "IndexOfMenuItemView",
            ["StardewValley.Tool::initialParentTileIndex"] = "InitialParentTileIndex",
            ["StardewValley.Tool::instantUse"] = "InstantUse",
            ["StardewValley.Tool::netName"] = "BaseName",
            ["StardewValley.Tool::stackable"] = "Stackable",
            ["StardewValley.Tool::upgradeLevel"] = "UpgradeLevel"
        };

        /// <summary>Describes the diagnostic rule covered by the analyzer.</summary>
        private readonly IDictionary<string, DiagnosticDescriptor> Rules = new Dictionary<string, DiagnosticDescriptor>
        {
            ["SMAPI001"] = new DiagnosticDescriptor(
                id: "SMAPI001",
                title: "Netcode types shouldn't be implicitly converted",
                messageFormat: "This implicitly converts '{0}' from {1} to {2}, but {1} has unintuitive implicit conversion rules. Consider comparing against the actual value instead to avoid bugs. See https://smapi.io/buildmsg/SMAPI001 for details.",
                category: "SMAPI.CommonErrors",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: "",
                helpLinkUri: "https://smapi.io/buildmsg/SMAPI001"
            ),
            ["SMAPI002"] = new DiagnosticDescriptor(
                id: "SMAPI002",
                title: "Avoid Netcode types when possible",
                messageFormat: "'{0}' is a {1} field; consider using the {2} property instead. See https://smapi.io/buildmsg/SMAPI002 for details.",
                category: "SMAPI.CommonErrors",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: "",
                helpLinkUri: "https://smapi.io/buildmsg/SMAPI001"
            )
        };


        /*********
        ** Accessors
        *********/
        /// <summary>The descriptors for the diagnostics that this analyzer is capable of producing.</summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public NetFieldAnalyzer()
        {
            this.SupportedDiagnostics = ImmutableArray.CreateRange(this.Rules.Values);
        }

        /// <summary>Called once at session start to register actions in the analysis context.</summary>
        /// <param name="context">The analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            // SMAPI002: avoid net fields if possible
            context.RegisterSyntaxNodeAction(
                this.AnalyzeAvoidableNetField,
                SyntaxKind.SimpleMemberAccessExpression
            );

            // SMAPI001: avoid implicit net field conversion
            context.RegisterSyntaxNodeAction(
                this.AnalyseNetFieldConversions,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression,
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Analyse a syntax node and add a diagnostic message if it references a net field when there's a non-net equivalent available.</summary>
        /// <param name="context">The analysis context.</param>
        private void AnalyzeAvoidableNetField(SyntaxNodeAnalysisContext context)
        {
            try
            {
                // check member type
                MemberAccessExpressionSyntax node = (MemberAccessExpressionSyntax)context.Node;
                TypeInfo memberType = context.SemanticModel.GetTypeInfo(node);
                if (!this.IsNetType(memberType.Type))
                    return;

                // get reference info
                ITypeSymbol declaringType = context.SemanticModel.GetTypeInfo(node.Expression).Type;
                string propertyName = node.Name.Identifier.Text;

                // suggest replacement
                if (this.NetFieldWrapperProperties.TryGetValue($"{declaringType}::{propertyName}", out string suggestedPropertyName))
                {
                    context.ReportDiagnostic(Diagnostic.Create(this.Rules["SMAPI002"], context.Node.GetLocation(), node, memberType.Type.Name, suggestedPropertyName));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed processing expression: '{context.Node}'. Exception details: {ex.ToString().Replace('\r', ' ').Replace('\n', ' ')}");
            }
        }

        /// <summary>Analyse a syntax node and add a diagnostic message if it implicitly converts a net field.</summary>
        /// <param name="context">The analysis context.</param>
        private void AnalyseNetFieldConversions(SyntaxNodeAnalysisContext context)
        {
            try
            {
                BinaryExpressionSyntax node = (BinaryExpressionSyntax)context.Node;
                bool leftHasWarning = this.WarnIfOperandImplicitlyConvertsNetField(context, node.Left);
                if (!leftHasWarning)
                    this.WarnIfOperandImplicitlyConvertsNetField(context, node.Right);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed processing expression: '{context.Node}'. Exception details: {ex.ToString().Replace('\r', ' ').Replace('\n', ' ')}");
            }
        }

        /// <summary>Analyse one operand in a binary expression (like <c>a</c> and <c>b</c> in <c>a == b</c>) and add a diagnostic message if applicable.</summary>
        /// <param name="context">The analysis context.</param>
        /// <param name="operand">The operand expression.</param>
        /// <returns>Returns whether a diagnostic message was raised.</returns>
        private bool WarnIfOperandImplicitlyConvertsNetField(SyntaxNodeAnalysisContext context, ExpressionSyntax operand)
        {
            TypeInfo operandType = context.SemanticModel.GetTypeInfo(operand);
            if (this.IsNetType(operandType.Type) && !this.IsNetType(operandType.ConvertedType))
            {
                context.ReportDiagnostic(Diagnostic.Create(this.Rules["SMAPI001"], context.Node.GetLocation(), operand, operandType.Type.Name, operandType.ConvertedType));
                return true;
            }

            return false;
        }

        /// <summary>Get whether a type symbol references a <c>Netcode</c> type.</summary>
        /// <param name="typeSymbol">The type symbol.</param>
        private bool IsNetType(ITypeSymbol typeSymbol)
        {
            return typeSymbol?.ContainingNamespace?.Name == NetFieldAnalyzer.NetcodeNamespace;
        }
    }
}
