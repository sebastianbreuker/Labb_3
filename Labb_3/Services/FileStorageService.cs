using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Labb_3.Services
{
   
    public class FileStorageService : IStorageService
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly string _defaultDirectory;
        private readonly string _defaultFilePath;

        public FileStorageService(string? baseDirectory = null, string? fileName = null)
        {
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            _defaultDirectory = baseDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuizConfigurator");
            _defaultFilePath = Path.Combine(_defaultDirectory, fileName ?? "packs.json");
        }

        public async Task<IReadOnlyList<QuestionPack>> LoadPacksAsync()
        {
            try
            {
                if (File.Exists(_defaultFilePath))
                {
                    await using var stream = new FileStream(_defaultFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var packs = await JsonSerializer.DeserializeAsync<List<QuestionPack>>(stream, _serializerOptions).ConfigureAwait(false);
                    return packs ?? new List<QuestionPack>();
                }
            }
            catch
            {
            }

            return GetSeedPacks();
        }

        public async Task SavePacksAsync(IEnumerable<QuestionPack> packs)
        {
            try
            {
                Directory.CreateDirectory(_defaultDirectory);
                await using var stream = new FileStream(_defaultFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(stream, packs.ToList(), _serializerOptions).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        public async Task<QuestionPack?> LoadPackFromFileAsync(string filePath)
        {
            try
            {
                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return await JsonSerializer.DeserializeAsync<QuestionPack>(stream, _serializerOptions).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        public async Task SavePackToFileAsync(QuestionPack pack, string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(stream, pack, _serializerOptions).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private static IReadOnlyList<QuestionPack> GetSeedPacks()
        {
            return new List<QuestionPack>
            {
                new QuestionPack
                {
                    Name = "Default Question Pack",
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Text = "Vad är 1 + 1?",
                            Options = new List<AnswerOption>
                            {
                                new AnswerOption { Text = "2", isCorrect = true },
                                new AnswerOption { Text = "1", isCorrect = false },
                                new AnswerOption { Text = "3", isCorrect = false },
                                new AnswerOption { Text = "4", isCorrect = false }
                            }
                        }
                    }
                }
            };
        }
    }
}
