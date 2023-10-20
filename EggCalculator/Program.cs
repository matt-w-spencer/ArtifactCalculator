using System;
using System.Collections.Generic;
using System.IO;
using EggCalculator.model;
using Newtonsoft.Json;

namespace EggCalculator
{
    public class Program
    {
        public static string? jsonString { get; private set; }
        public static Dictionary<string, List<int>> intermediateResults = new Dictionary<string, List<int>>();

        public static string outputText = ""; // Store output text here

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please provide the artifact name.");
                return;
            }

            string artifactName = args[0];

            outputText += "Starting the calculation...\n";

            string jsonFilePath = "new_extracted_data.json";
            StreamReader reader = new StreamReader(jsonFilePath);

            jsonString = reader.ReadToEnd();

            RootObject? root = JsonConvert.DeserializeObject<RootObject>(jsonString);

            var artifacts = root.Artifacts;

            bool artifactFound = false;

            
            foreach (var family in artifacts) // Iterate over families
            {
                intermediateResults.Clear();
                foreach (var artifact in family.Value) // Iterate over artifacts within a family
                {
                    if (artifact.Key.Equals(artifactName, StringComparison.OrdinalIgnoreCase)) // Case insensitive check
                    {
                        artifactFound = true;

                        Console.WriteLine($"{artifact.Key}:");

                        var ingredientsRequired = CalculateIngredients(artifact.Value, artifacts);

                        foreach (var ingredient in ingredientsRequired)
                        {
                            Console.WriteLine($"    {ingredient.Value} {ingredient.Key}");
                        }

                        Console.WriteLine();
                    }
                }
            }

            if (!artifactFound)
            {
                Console.WriteLine($"Artifact named '{artifactName}' was not found in the data.");
            }
            // Save to a text file
            using StreamWriter writer = new StreamWriter("Calculated Ingredients.txt");
            writer.Write(outputText);
        }
        private static Dictionary<string, int> CalculateIngredients(Artifact artifact, Dictionary<string, Dictionary<string, Artifact>> artifacts)
        {
            Dictionary<string, int> ingredientsRequired = new Dictionary<string, int>();

            outputText += $"Calculating ingredients for {artifact.family} tier {artifact.tier}...\n";

            foreach (var ingredient in artifact.Ingredients)
            {
                var resultsForThisIngredient = ProcessIngredient(ingredient, artifacts);

                foreach (var result in resultsForThisIngredient)
                {
                    if (ingredientsRequired.ContainsKey(result.Key))
                    {
                        ingredientsRequired[result.Key] += result.Value;
                    }
                    else
                    {
                        ingredientsRequired[result.Key] = result.Value;
                    }
                }
            }

            return ingredientsRequired;
        }

        // ... [rest of your code above this]

        private static Dictionary<string, int> ProcessIngredient(KeyValuePair<string, Ingredient> ingredient, Dictionary<string, Dictionary<string, Artifact>> artifacts)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();

            string familyName = GetFamilyName(ingredient.Key, artifacts);
            Artifact ingredientArtifact = artifacts[familyName][ingredient.Key];

            foreach (var innerIngredient in ingredientArtifact.Ingredients)
            {
                if (innerIngredient.Value.tier == 1)
                {
                    results = ProcessTier1Ingredient(innerIngredient, ingredient, results);
                }
                else
                {
                    results = ProcessHigherTierIngredient(innerIngredient, ingredient, artifacts, results);
                }
            }
            return results;
        }

        private static Dictionary<string, int> ProcessTier1Ingredient(KeyValuePair<string, Ingredient> innerIngredient, KeyValuePair<string, Ingredient> ingredient, Dictionary<string, int> results)
        {

            if (results.ContainsKey(innerIngredient.Key))
            {
                results[innerIngredient.Key] += innerIngredient.Value.IngredientCount * ingredient.Value.IngredientCount;
            }
            else
            {
                results[innerIngredient.Key] = innerIngredient.Value.IngredientCount * ingredient.Value.IngredientCount;
            }

            return results;
        }

        private static Dictionary<string, int> ProcessHigherTierIngredient(KeyValuePair<string, Ingredient> innerIngredient, KeyValuePair<string, Ingredient> ingredient, Dictionary<string, Dictionary<string, Artifact>> artifacts, Dictionary<string, int> results)
        {

            var deeperIngredients = ProcessIngredient(new KeyValuePair<string, Ingredient>(innerIngredient.Key, new Ingredient
            {
                tier = innerIngredient.Value.tier,
                IngredientCount = innerIngredient.Value.IngredientCount * ingredient.Value.IngredientCount
            }), artifacts);

            foreach (var deeperIngredient in deeperIngredients)
            {
                if (results.ContainsKey(deeperIngredient.Key))
                {
                    results[deeperIngredient.Key] += deeperIngredient.Value;
                }
                else
                {
                    results[deeperIngredient.Key] = deeperIngredient.Value;
                }
            }

            return results;
        }

        // ... [rest of your code below this]

        private static string GetFamilyName(string ingredientName, Dictionary<string, Dictionary<string, Artifact>> artifacts)
        {
            foreach (var family in artifacts)
            {
                if (family.Value.ContainsKey(ingredientName))
                {
                    string familyName = family.Value[ingredientName].family;
                    return familyName;
                }
            }
            return "Unknown"; // Default if not found, though it shouldn't happen if your data is consistent.
        }
    }
}