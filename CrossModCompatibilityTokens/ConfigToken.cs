#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CrossModCompatibilityTokens
{
    internal class ConfigToken
    {
        private readonly Dictionary<string, Dictionary<string, string?>> cachedValues = new();
        private bool shouldUpdate;

        /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
        /// <remarks>Default false.</remarks>
        public bool AllowsInput()
        {
            return true;
        }

        /// <summary>Whether the token requires input arguments to work, and does not provide values without it (see <see cref="AllowsInput"/>).</summary>
        /// <remarks>Default false.</remarks>
        public bool RequiresInput()
        {
            return true;
        }

        /// <summary>Whether the token may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <remarks>Default true.</remarks>
        public bool CanHaveMultipleValues(string? input = null)
        {
            return true;
        }

        /// <summary>Validate that the provided input arguments are valid.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        /// <remarks>Default true.</remarks>
        public bool TryValidateInput(string? input, [NotNullWhen(false)] out string? error)
        {
            string[] split = input?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray() ??
                             [];
            if (split.Length != 2)
            {
                error = "Expected two arguments.";
                return false;
            }

            if (!ModEntry.ModList.ContainsKey(split[0]) && !ModEntry.PackList.ContainsKey(split[0]))
            {
                error = "Mod or pack not found.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Notify Content Patcher once when a new config value is added to the session cache.
        /// Config values are intentionally not polled after their first read; restart the game
        /// after changing another mod's config to refresh CMCT values.
        /// </summary>
        public bool UpdateContext()
        {
            if (!shouldUpdate)
            {
                return false;
            }

            shouldUpdate = false;
            return true;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public bool IsReady()
        {
            return ModEntry.ModList.Any() || ModEntry.PackList.Any();
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if any.</param>
        public IEnumerable<string> GetValues(string? input)
        {
            if (input is null)
            {
                yield break;
            }

            var split = input.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (split.Length != 2)
            {
                yield break;
            }

            var uniqueID = split[0];
            var configKey = split[1];

            if (!cachedValues.TryGetValue(uniqueID, out var modConfig))
            {
                modConfig = new Dictionary<string, string?>();
                cachedValues.Add(uniqueID, modConfig);
            }

            if (!modConfig.TryGetValue(configKey, out var configValue))
            {
                configValue = ModEntry.GrabConfigValue(uniqueID, configKey)?.ToObject<string>();
                modConfig.Add(configKey, configValue);
                shouldUpdate = true;
            }

            if (string.IsNullOrEmpty(configValue))
            {
                yield break;
            }

            foreach (var value in configValue.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
            {
                yield return value;
            }
        }
    }
}
