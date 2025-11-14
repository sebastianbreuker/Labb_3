using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Labb_3.Services
{
    public class OpenTriviaService
    {
        private static readonly HttpClient _httpClient = CreateHttpClient();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<IReadOnlyList<TriviaCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await _httpClient.GetAsync("api_category.php", cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var result = await JsonSerializer.DeserializeAsync<CategoryResponse>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
                if (result?.TriviaCategories == null)
                {
                    return new List<TriviaCategory>();
                }
                return result.TriviaCategories.Select(c => new TriviaCategory(c.Id, WebUtility.HtmlDecode(c.Name))).ToList();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                throw new OpenTriviaException("Failed to load categories from Open Trivia Database.", ex);
            }
        }

        public async Task<List<Question>> ImportQuestionsAsync(
            int amount,
            TriviaCategory category,
            Difficulty? difficulty,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(category);
            if (amount < 1 || amount > 50)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be between 1 and 50.");
            }

            try
            {
                var query = $"api.php?amount={amount}&category={category.Id}&type=multiple";
                if (difficulty.HasValue)
                {
                    query += $"&difficulty={difficulty.Value.ToString().ToLowerInvariant()}";
                }

                using var response = await _httpClient.GetAsync(query, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var result = await JsonSerializer.DeserializeAsync<QuestionsResponse>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false)
                             ?? throw new OpenTriviaException("Unexpected response from Open Trivia Database.");

                if (result.ResponseCode != 0)
                {
                    throw new OpenTriviaApiException(result.ResponseCode);
                }

                if (result.Results is null || result.Results.Count == 0)
                {
                    throw new OpenTriviaException("No questions were returned from the Open Trivia Database.");
                }

                var random = new Random();
                var questions = new List<Question>();

                foreach (var apiQuestion in result.Results)
                {
                    var questionText = WebUtility.HtmlDecode(apiQuestion.Question ?? string.Empty);
                    var correctAnswer = WebUtility.HtmlDecode(apiQuestion.CorrectAnswer ?? string.Empty);
                    var incorrectAnswers = apiQuestion.IncorrectAnswers?
                        .Select(a => WebUtility.HtmlDecode(a ?? string.Empty))
                        .ToList() ?? new List<string>();

                    if (string.IsNullOrWhiteSpace(questionText) || string.IsNullOrWhiteSpace(correctAnswer))
                    {
                        continue;
                    }

                    var options = new List<AnswerOption>
                    {
                        new AnswerOption { Text = correctAnswer, isCorrect = true }
                    };

                    options.AddRange(incorrectAnswers.Select(answer => new AnswerOption
                    {
                        Text = answer,
                        isCorrect = false
                    }));

                    if (options.Count < 2)
                    {
                        continue;
                    }

                    options = options.OrderBy(_ => random.Next()).ToList();

                    questions.Add(new Question
                    {
                        Text = questionText,
                        Options = options
                    });
                }

                if (questions.Count == 0)
                {
                    throw new OpenTriviaException("The Open Trivia Database returned questions that could not be processed.");
                }

                return questions;
            }
            catch (OpenTriviaApiException)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                throw new OpenTriviaException("Failed to import questions from Open Trivia Database.", ex);
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://opentdb.com/")
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Labb_3/1.0");
            return client;
        }

        private sealed class CategoryResponse
        {
            [JsonPropertyName("trivia_categories")]
            public List<CategoryDto>? TriviaCategories { get; set; }
        }

        private sealed class CategoryDto
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }

        private sealed class QuestionsResponse
        {
            [JsonPropertyName("response_code")]
            public int ResponseCode { get; set; }
            
            [JsonPropertyName("results")]
            public List<QuestionDto>? Results { get; set; }
        }

        private sealed class QuestionDto
        {
            [JsonPropertyName("question")]
            public string? Question { get; set; }
            
            [JsonPropertyName("correct_answer")]
            public string? CorrectAnswer { get; set; }
            
            [JsonPropertyName("incorrect_answers")]
            public List<string>? IncorrectAnswers { get; set; }
        }
    }

    public record TriviaCategory(int Id, string Name);

    public class OpenTriviaException : Exception
    {
        public OpenTriviaException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }

    public class OpenTriviaApiException : OpenTriviaException
    {
        public int ResponseCode { get; }

        public OpenTriviaApiException(int responseCode)
            : base(GetMessage(responseCode))
        {
            ResponseCode = responseCode;
        }

        private static string GetMessage(int code) => code switch
        {
            1 => "The API could not return enough questions for your query. Try reducing the amount or relaxing the filters.",
            2 => "Invalid parameter supplied to the Open Trivia Database API.",
            3 => "Token not found. Please try again.",
            4 => "Token empty. Please try again.",
            _ => $"Unexpected response from Open Trivia Database (code {code})."
        };
    }
}


