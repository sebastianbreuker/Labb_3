using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Labb_3.Services
{
    public class JsonStorageService
    {
        private readonly string _storageDirectory;
        private readonly string _storageFilePath;
        private readonly string _defaultDataPath;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public JsonStorageService()
        {
            _storageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Labb_3");
            _storageFilePath = Path.Combine(_storageDirectory, "question_packs.json");
            _defaultDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON", "default-pack.json");

            Directory.CreateDirectory(_storageDirectory);
        }

        public IReadOnlyList<QuestionPack> LoadPacks()
        {
            if (File.Exists(_storageFilePath))
            {
                return DeserializeFromFile(_storageFilePath);
            }

            if (File.Exists(_defaultDataPath))
            {
                var packs = DeserializeFromFile(_defaultDataPath);
                if (packs.Count > 0)
                {
                    SavePacks(packs);
                    return packs;
                }
            }

            return new List<QuestionPack>
            {
                new QuestionPack
                {
                    Name = "Default Question Pack",
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Text = "New Question",
                            Options = new List<AnswerOption>
                            {
                                new AnswerOption { Text = "", isCorrect = true },
                                new AnswerOption { Text = "", isCorrect = false },
                                new AnswerOption { Text = "", isCorrect = false },
                                new AnswerOption { Text = "", isCorrect = false }
                            }
                        }
                    }
                }
            };
        }

        public void SavePacks(IEnumerable<QuestionPack> packs)
        {
            try
            {
                Directory.CreateDirectory(_storageDirectory);
                var packList = packs.ToList();
                var json = JsonSerializer.Serialize(packList, _jsonOptions);
                File.WriteAllText(_storageFilePath, json);
            }
            catch
            {
            }
        }

        private IReadOnlyList<QuestionPack> DeserializeFromFile(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var packs = JsonSerializer.Deserialize<List<QuestionPack>>(json, _jsonOptions);
                return packs ?? new List<QuestionPack>();
            }
            catch
            {
                return Array.Empty<QuestionPack>();
            }
        }
    }
}



