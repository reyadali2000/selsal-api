using System.Text.Json;

namespace WadiAlAin.Application.Common.Helpers
{
    public static class JsonExtensions
    {
        public static bool? GetBoolOrNull(this JsonElement elem, string prop1, string? prop2 = null)
        {
            if (elem.TryGetProperty(prop1, out var p) || (prop2 != null && elem.TryGetProperty(prop2, out p)))
            {
                if (p.ValueKind == JsonValueKind.True) return true;
                if (p.ValueKind == JsonValueKind.False) return false;
                if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var num)) return num != 0;
                if (p.ValueKind == JsonValueKind.String)
                {
                    var s = p.GetString()?.Trim().ToLowerInvariant();
                    if (s == "true" || s == "1") return true;
                    if (s == "false" || s == "0") return false;
                }
            }
            return null;
        }

        public static bool GetBool(this JsonElement elem, string prop1, string? prop2 = null, bool defaultVal = false)
        {
            return elem.GetBoolOrNull(prop1, prop2) ?? defaultVal;
        }

        public static int? GetIntOrNull(this JsonElement elem, string prop1, string? prop2 = null)
        {
            if (elem.TryGetProperty(prop1, out var p) || (prop2 != null && elem.TryGetProperty(prop2, out p)))
            {
                if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v)) return v;
                if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var sv)) return sv;
            }
            return null;
        }

        public static int GetInt(this JsonElement elem, string prop1, string? prop2 = null, int defaultVal = 0)
        {
            return elem.GetIntOrNull(prop1, prop2) ?? defaultVal;
        }

        public static decimal? GetDecimalOrNull(this JsonElement elem, string prop1, string? prop2 = null)
        {
            if (elem.TryGetProperty(prop1, out var p) || (prop2 != null && elem.TryGetProperty(prop2, out p)))
            {
                if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var v)) return v;
                if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var sv)) return sv;
            }
            return null;
        }

        public static decimal GetDecimal(this JsonElement elem, string prop1, string? prop2 = null, decimal defaultVal = 0m)
        {
            return elem.GetDecimalOrNull(prop1, prop2) ?? defaultVal;
        }

        public static string? GetStringOrNull(this JsonElement elem, string prop1, string? prop2 = null, string? prop3 = null)
        {
            if (elem.TryGetProperty(prop1, out var p) || 
                (prop2 != null && elem.TryGetProperty(prop2, out p)) ||
                (prop3 != null && elem.TryGetProperty(prop3, out p)))
            {
                if (p.ValueKind == JsonValueKind.String) return p.GetString();
                if (p.ValueKind == JsonValueKind.Number) return p.ToString();
                if (p.ValueKind == JsonValueKind.True) return "true";
                if (p.ValueKind == JsonValueKind.False) return "false";
                if (p.ValueKind == JsonValueKind.Null) return null;
            }
            return null;
        }

        public static string GetString(this JsonElement elem, string prop1, string? prop2 = null, string? prop3 = null, string defaultVal = "")
        {
            return elem.GetStringOrNull(prop1, prop2, prop3) ?? defaultVal;
        }
    }
}
