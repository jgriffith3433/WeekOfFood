using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace ContainerNinja.Contracts.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UnitType
    {
        [Display(Name = "")]
        [EnumMember(Value = "")]
        None = 0,
        
        [Display(Name = "Bulk")]
        [EnumMember(Value = "Bulk")]
        Bulk = 1,
        
        [Display(Name = "Ounce")]
        [EnumMember(Value = "Ounce")]
        Ounce = 2,
        
        [Display(Name = "Teaspoon")]
        [EnumMember(Value = "Teaspoon")]
        Teaspoon = 3,
        
        [Display(Name = "Tablespoon")]
        [EnumMember(Value = "Tablespoon")]
        Tablespoon = 4,
        
        [Display(Name = "Pound")]
        [EnumMember(Value = "Pound")]
        Pound = 5,
        
        [Display(Name = "Cup")]
        [EnumMember(Value = "Cup")]
        Cup = 6,
        
        [Display(Name = "Cloves")]
        [EnumMember(Value = "Cloves")]
        Cloves = 7,
        
        [Display(Name = "Can")]
        [EnumMember(Value = "Can")]
        Can = 8,
        
        [Display(Name = "Whole")]
        [EnumMember(Value = "Whole")]
        Whole = 9,
        
        [Display(Name = "Package")]
        [EnumMember(Value = "Package")]
        Package = 10,
        
        [Display(Name = "Bar")]
        [EnumMember(Value = "Bar")]
        Bar = 11,
        
        [Display(Name = "Bun")]
        [EnumMember(Value = "Bun")]
        Bun = 12,
        
        [Display(Name = "Bottle")]
        [EnumMember(Value = "Bottle")]
        Bottle = 13,
        
        [Display(Name = "Slice")]
        [EnumMember(Value = "Slice")]
        Slice = 14,
        
        [Display(Name = "Box")]
        [EnumMember(Value = "Box")]
        Box = 15,
        
        [Display(Name = "Bag")]
        [EnumMember(Value = "Bag")]
        Bag = 16,
        
        [Display(Name = "Gallon")]
        [EnumMember(Value = "Gallon")]
        Gallon = 17,
        
        [Display(Name = "Gram")]
        [EnumMember(Value = "Gram")]
        Gram = 18,
        
        [Display(Name = "Milliliters")]
        [EnumMember(Value = "Milliliters")]
        Milliliters = 19,
        
        [Display(Name = "Leaves")]
        [EnumMember(Value = "Leaves")]
        Leaves = 20,
        
        [Display(Name = "Handful")]
        [EnumMember(Value = "Handful")]
        Handful = 21,
        
        [Display(Name = "Packet")]
        [EnumMember(Value = "Packet")]
        Packet = 22,
        
        [Display(Name = "Count")]
        [EnumMember(Value = "Count")]
        Count = 23,
    }

    public static class UnitTypeExtensionMethods
    {
        public static UnitType UnitTypeFromString(this string unitTypeStr)
        {
            switch (unitTypeStr.ToLower())
            {
                case "":
                case "none":
                    return UnitType.None;
                case "bulk":
                    return UnitType.Bulk;
                case "ounce":
                case "ounces":
                    return UnitType.Ounce;
                case "teaspoon":
                case "teaspoons":
                    return UnitType.Teaspoon;
                case "tablespoon":
                case "tablespoons":
                    return UnitType.Tablespoon;
                case "pound":
                case "pounds":
                    return UnitType.Pound;
                case "cup":
                case "cups":
                    return UnitType.Cup;
                case "clove":
                case "cloves":
                    return UnitType.Cloves;
                case "can":
                case "cans":
                    return UnitType.Can;
                case "whole":
                case "wholes":
                    return UnitType.Whole;
                case "package":
                case "packages":
                    return UnitType.Package;
                case "bar":
                case "bars":
                    return UnitType.Bar;
                case "bun":
                case "buns":
                    return UnitType.Bun;
                case "bottle":
                case "bottles":
                    return UnitType.Bottle;
                case "slice":
                case "slices":
                    return UnitType.Slice;
                case "box":
                case "boxes":
                    return UnitType.Box;
                case "bag":
                case "bags":
                    return UnitType.Bag;
                case "gallon":
                case "gallons":
                    return UnitType.Gallon;
                case "gram":
                case "grams":
                    return UnitType.Gram;
                case "milliliter":
                case "milliliters":
                    return UnitType.Milliliters;
                case "leaf":
                case "leaves":
                    return UnitType.Leaves;
                case "handful":
                case "handfuls":
                    return UnitType.Handful;
                case "packet":
                case "packets":
                    return UnitType.Packet;
                case "count":
                case "counts":
                    return UnitType.Count;
                default:
                    return UnitType.None;
            }
        }
    }
}
