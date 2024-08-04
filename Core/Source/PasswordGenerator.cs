using System.Security.Cryptography;

namespace Core.Source;

public static class PasswordGenerator
{
    [Flags]
    public enum Filter
    {
        Lowercase = 1,
        Uppercase = 2,
        Number = 4,
        Specific = 8,
        All = ~0
    }
    
    private const string UppercaseCharacters = "abcdefgijkmnpqrstwxyz";
    private const string LowercaseCharacters = "ABCDEFGHJKLMNPQRSTWXYZ";
    private const string Numbers = "123456789";
    private const string SpecificSymbols = "!@#$%";

    public static List<string?>? GeneratePack(byte length, byte quantity = 5, Filter filter = Filter.All)
    {
        if (length == 0)
        {
            return null;
        }
        
        var results = new HashSet<string?>();
        for (byte i = 0; i < quantity; i++)
        {
            results.Add(GenerateSingle(length, filter));
        }

        return results.ToList();
    }

    public static string? GenerateSingle(byte length, Filter filter = Filter.All)
    {
        if (length == 0)
        {
            return null;
        }

        var possibleGroups = new List<char[]>();
        
        if(filter.HasFlag(Filter.Lowercase))
            possibleGroups.Add(UppercaseCharacters.ToCharArray());
        
        if(filter.HasFlag(Filter.Uppercase))
            possibleGroups.Add(LowercaseCharacters.ToCharArray());
        
        if(filter.HasFlag(Filter.Number))
            possibleGroups.Add(Numbers.ToCharArray());
        
        if(filter.HasFlag(Filter.Specific))
            possibleGroups.Add(SpecificSymbols.ToCharArray());
        
        return Generate(possibleGroups.ToArray(), length);
    }   

    private static string Generate(char[][] possibleGroups, byte length)
    {
        // Use this array to track the number of unused characters in each character group.
        var charsLeftInGroup = new byte[possibleGroups.Length];

        // Initially, all characters in each group are not used.
        for (var i = 0; i < charsLeftInGroup.Length; i++)
        {
            charsLeftInGroup[i] = (byte)possibleGroups[i].Length;
        }

        // Use this array to track (iterate through) unused character groups.
        var unusedGroups = new byte[possibleGroups.Length];

        // Initially, all character groups are not used.
        for (var i = 0; i < unusedGroups.Length; i++)
        {
            unusedGroups[i] = (byte)i;
        }

        // Generating random seed
        var randomBytes = new byte[4];
        var bytesGenerator = RandomNumberGenerator.Create();
        bytesGenerator.GetBytes(randomBytes);

        var seed = (randomBytes[0] & 0x7f) << 24 |
                   randomBytes[1] << 16 |
                   randomBytes[2] << 8 |
                   randomBytes[3];

        var random = new Random(seed);

        var result = new char[length];

        // some indexes
        var lastGroupIdx = unusedGroups.Length - 1;

        // Generate password characters one at a time.
        for (var i = 0; i < result.Length; i++)
        {
            // If only one character group remained unprocessed, process it; otherwise, 
            // pick a random character group from the unprocessed group list. To allow a 
            // special character to appear in the first position, increment the second 
            // parameter of the Next function call by one, i.e. lastLeftGroupsOrderIdx + 1.
            var nextLeftGroupsOrderIdx = lastGroupIdx == 0 ? 0 : random.Next(0, lastGroupIdx);

            // Get the actual index of the character group, from which we will pick the next character.
            var nextGroupIdx = unusedGroups[nextLeftGroupsOrderIdx];
            var lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

            // If only one unprocessed character is left, pick it; otherwise, get a 
            // random character from the unused character list.
            var nextCharIdx = lastCharIdx == 0 ? 0 : random.Next(0, lastCharIdx + 1);

            // Add this character to the password.
            result[i] = possibleGroups[nextGroupIdx][nextCharIdx];

            // If we processed the last character in this group, start over.
            if (lastCharIdx == 0)
            {
                charsLeftInGroup[nextGroupIdx] = (byte)possibleGroups[nextGroupIdx].Length;
            }
            else
            {
                if (lastCharIdx != nextCharIdx)
                {
                    (possibleGroups[nextGroupIdx][lastCharIdx], possibleGroups[nextGroupIdx][nextCharIdx]) = (possibleGroups[nextGroupIdx][nextCharIdx], possibleGroups[nextGroupIdx][lastCharIdx]);
                }

                charsLeftInGroup[nextGroupIdx]--;
            }

            // If we processed the last group, start all over.
            if (lastGroupIdx == 0)
            {
                lastGroupIdx = unusedGroups.Length - 1;
            }
            else
            {
                // Swap processed group with the last unprocessed group so that we don't pick it until we process all groups.
                if (lastGroupIdx != nextLeftGroupsOrderIdx)
                {
                    (unusedGroups[lastGroupIdx], unusedGroups[nextLeftGroupsOrderIdx]) = (unusedGroups[nextLeftGroupsOrderIdx], unusedGroups[lastGroupIdx]);
                }

                // Decrement the number of unprocessed groups.
                lastGroupIdx--;
            }
        }

        return new string(result);
    }
}