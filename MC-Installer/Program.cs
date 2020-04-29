using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RGInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    // Get the modpack info.
                    Console.WriteLine("Fetching modpack info...");
                    byte[] bytes = client.DownloadData("https://radioactive-gaming.github.io/minecraft/modpack.json");
                    Modpack modpackInfo = JsonConvert.DeserializeObject<Modpack>(Encoding.UTF8.GetString(bytes));

                    // Check if the modpack was read successfully.
                    if (modpackInfo == null)
                        throw new Exception("Modpack info failed to be read");

                    // Get the minecraft directory path. 
                    string minecraftDirectory = args.Length > 0 ?
                        args[0] :
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");

                    // Ensure the minecraft directory exists.
                    if (Directory.Exists(minecraftDirectory))
                        Console.WriteLine("Minecraft Directory found at \"{0}\"", minecraftDirectory);
                    else
                        throw new Exception("Minecraft Directory not found.");

                    // Get the mods directory. Create one if needed.
                    string modsDirectory = Path.Combine(minecraftDirectory, "mods");
                    Directory.CreateDirectory(modsDirectory);

                    // Get the versions directory. Something is wrong if it isn't present
                    string versionsDirectory = Path.Combine(minecraftDirectory, "versions");

                    if (!Directory.Exists(versionsDirectory))
                        throw new Exception("Versions Directory not found.");

                    // Check for the correct version of Forge.
                    string[] jsonFilePaths = Directory.GetFiles(versionsDirectory, "*.json", SearchOption.AllDirectories);

                    bool forgeFound = false;
                    for (int i = 0; i < jsonFilePaths.Length && !forgeFound; i++)
                    {
                        using (TextReader textReader = File.OpenText(jsonFilePaths[i]))
                        using (JsonReader jsonReader = new JsonTextReader(textReader))
                        {
                            JObject json = JObject.Load(jsonReader);

                            forgeFound = json?["id"].ToString() == modpackInfo.ForgeVersion;
                        }
                    }

                    if (forgeFound)
                        Console.WriteLine("The correct version of forge is installed.");
                    else
                        throw new Exception(string.Format("Please instal "));

                    // Download the mods
                    foreach (string modSource in modpackInfo.ModSources)
                    {
                        string filename = Path.GetFileName(modSource);
                        string filepath = Path.Combine(modsDirectory, filename);

                        if (File.Exists(filepath))
                        {
                            Console.WriteLine("{0} is already installed", filename);
                        }
                        else
                        {
                            Console.WriteLine("Downloading {0}...", filename);
                            client.DownloadFile(modSource, filepath);
                        }
                    }

                    Console.WriteLine("Download Complete!");
                }
            }
            catch (WebException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                Console.WriteLine(" - Response: {0}", e.Response?.ToString() ?? "No Response");
                Console.WriteLine("Please post a screenshot of this log to the RG discord for help");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                Console.WriteLine("Please post a screenshot of this log to the RG discord for help");
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        class Modpack
        {
            public string ForgeVersion { get; set; }
            public string[] ModSources { get; set; }
        }
    }
}
