﻿using System;
using System.Linq;

namespace ServiceModules.Internal;
internal static class StringExtensions {
    public static bool MatchWildcard(this string value, string check, char wildcard, StringComparison comparison = StringComparison.Ordinal) {
        var trimmedCheck = check.Trim(wildcard);
        return (check.StartsWith(wildcard), check.EndsWith(wildcard), trimmedCheck.Contains(wildcard)) switch {
            (var wildcardStart, var wildcardEnd, true) => MatchInnerWildcard(value, trimmedCheck.Split(wildcard), !wildcardStart, !wildcardEnd, comparison),
            (true, true, _) => value.Contains(trimmedCheck, comparison),
            (false, true, _) => value.StartsWith(trimmedCheck, comparison),
            (true, false, _) => value.EndsWith(trimmedCheck, comparison),
            (false, false, _) => value.Equals(check, comparison)
        };
    }

    private static bool MatchInnerWildcard(string value, string[] checkSegments, bool shouldMatchStart, bool shouldMatchEnd, StringComparison comparison) {
        if (checkSegments.Length == 0) {
            return false;
        }

        if (shouldMatchStart && !value.StartsWith(checkSegments.First(), comparison)) {
            return false;
        }
        if (shouldMatchEnd && !value.EndsWith(checkSegments.Last(), comparison)) {
            return false;
        }

        var valueIndex = 0;
        foreach (var segment in checkSegments) {
            valueIndex = value.IndexOf(segment, valueIndex, comparison);
            if (valueIndex < 0) {
                break;
            }
            valueIndex += segment.Length;
        }
        return valueIndex >= 0;
    }
}
