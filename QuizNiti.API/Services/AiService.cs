using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using QuizNiti.API.Data;
using QuizNiti.API.DTOs;
using QuizNiti.API.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using static QuizNiti.API.DTOS.HuggingFaceDTO;

namespace QuizNiti.API.Services
{
    public class AiService
    {
        // This is a placeholder for AI-related functionalities.
        // Actual implementation would go here.
        private readonly QuizNitiContext _db;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        readonly string? hfToken ;

        public AiService(QuizNitiContext db, HttpClient httpClient, IConfiguration config)
        {
            _db = db;
            _httpClient = httpClient;
            _config = config;

             hfToken = Environment.GetEnvironmentVariable("HF_TOKEN")
              ?? _config["AISettings:HuggingFaceToken"];
        }

        public async Task<List<Questions>> AIWillGenerateQuestionsAsync(string topic, string difficulty, int count)
        {
            // Promt for logic for AI question generation

            string prompt = $@"
            Generate {count} multiple-choice current affairs questions 
            about the topic '{topic}' with '{difficulty}' difficulty.

            For each question, include:
            - The question text
            - 4 options
            - The correct answer
            - A short 'DidYouKnow' paragraph (1-2 sentences) that gives an interesting or mind-blowing fact related to that question.

            Return the result strictly in this JSON format:
            [
              {{
                ""QuestionText"": ""<question>"",
                ""Options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
                ""CorrectAnswer"": ""<correct option>"",
                ""DidYouKnow"": ""<fascinating fact or trivia>"",
                ""Topic"": ""{topic}"",
                ""Difficulty"": ""{difficulty}""
              }}
            ]
            ";

            string provider = _config["AISettings:Provider"] ?? "Ollama";
            string aiResponse;

            if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                //Use Ollama for Local Call Because Free to use but server side hosting not available yet for Free  
                aiResponse = await Call_Locally_OllamaAsync(prompt);
            }
            else 
            {
                // Use HuggingFace for Cloud-Based AI Model Hosting for server side calls it is Free
                aiResponse = await Call_HuggingFace_API_Async(prompt);
            }



            // Send the prompt to Ollama ---
            //var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", new
            //{
            //    model = "phi3", // Lite version Using Now later we can change  to llama3
            //    prompt = prompt
            //});

            //if (!response.IsSuccessStatusCode)
            //    throw new Exception("AI model failed to generate questions.");

            //var result = await response.Content.ReadAsStringAsync();

            // Extract JSON output ---
            //var startIndex = aiResponse.IndexOf("[");
            //var endIndex = aiResponse.LastIndexOf("]");
            //var json = aiResponse.Substring(startIndex, endIndex - startIndex + 1);

            /*  This AI Json Responce We are getting we have to Extract in proper format

            //{ "id":"95dc7e2746fe4604822876c0a2145d6f","object":"chat.completion","created":1763144555,"model":"meta-llama/llama-3.1-8b-instruct",
            //"choices":[{ "index":0,
            //"message":{ "role":"assistant",
            //"content":"[\n  {\n    \"QuestionText\": \"What is the primary goal of the DeepMind AI project, AlphaGo?\",\n
            //\"Options\": [\"To develop a human-like robot\", \"To create a chess-playing computer\", \"To beat the world champion in Go\", \"To develop a self-driving car\"],\n
            //\"CorrectAnswer\": \"C\",\n
            //\"DidYouKnow\": \"AlphaGo's victory over a human Go champion was a significant milestone in AI research, as it marked the first time a computer had beaten a world champion in a complex game.\",\n
            //\"Topic\": \"Artificial Intelligence\",\n    \"Difficulty\": \"Easy\"\n  },\n
            //{\n    \"QuestionText\": \"Which of the following is a key application of Natural Language Processing (NLP)?\",\n
            //\"Options\": [\"Speech recognition\", \"Image recognition\", \"Facial recognition\", \"Genetic engineering\"],\n
            //\"CorrectAnswer\": \"A\",\n
            //\"DidYouKnow\": \"NLP has many real-world applications, including virtual assistants like Siri and Alexa, language translation tools, and text summarization services.\",\n
            //\"Topic\": \"Artificial Intelligence\",\n    \"Difficulty\": \"Easy\"\n  },\n
            //{\n    \"QuestionText\": \"What is the primary function of a Neural Network in AI?\",\n
            //\"Options\": [\"To perform logical operations\", \"To learn from data and make predictions\", \"To store and retrieve data\", \"To generate random numbers\"],\n
            //\"CorrectAnswer\": \"B\",\n    \"DidYouKnow\": \"Neural Networks are inspired by the structure and function of the human brain and are a key component of many AI systems, including image and speech recognition models.\",\n
            //\"Topic\": \"Artificial Intelligence\",\n
            //\"Difficulty\": \"Easy\"\n  }\n]
            //"},
            //"finish_reason":"stop","content_filter_results":{ "hate":{ "filtered":false},
            //"self_harm":{ "filtered":false},"sexual":{ "filtered":false},"violence":{ "filtered":false},
            //"jailbreak":{ "filtered":false,"detected":false},"profanity":{ "filtered":false,"detected":false} } }],
            //"usage":{ "prompt_tokens":203,"completion_tokens":353,"total_tokens":556,"prompt_tokens_details":null,
            //"completion_tokens_details":null},"system_fingerprint":""}

            */
            
            // Root 
            var root = JsonSerializer.Deserialize<HFChatRoot>(aiResponse);

            if (root == null || root.choices == null || root.choices.Count == 0)
                throw new Exception("Invalid HuggingFace Not Getting Choise from AI API.");

            // Extract assistant message content (this contains your MCQ JSON)

            var content = root.choices[0].message.content;

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("HuggingFace response does not contain content.");

            // Extract JSON array from inside assistant's message
            var startIndex = content.IndexOf("[");
            var endIndex = content.LastIndexOf("]");

            if (startIndex == -1 || endIndex == -1)
                throw new Exception("Could not find MCQ JSON array inside HuggingFace response.");

            var json = content.Substring(startIndex, endIndex - startIndex + 1);

            // Convert inner JSON into Questions
            var aiQuestions = JsonSerializer.Deserialize<List<Questions>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (aiQuestions == null || aiQuestions.Count == 0)
                throw new Exception("Failed to parse MCQs from HuggingFace response.");

            // Deserialize JSON into Question objects ---
            //var DeserializeToQuestions = JsonSerializer.Deserialize<List<Questions>>(json, new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //})?? new List <Questions>();

            //Retrive the Required Data from my AI Response
            foreach (var q in aiQuestions)
            {
                if (q.Options != null && q.Options.Count == 4)
                {
                    q.OptionA = q.Options[0];
                    q.OptionB = q.Options[1];
                    q.OptionC = q.Options[2];
                    q.OptionD = q.Options[3];
                }



                // Convert CorrectAnswer (A/B/C/D) → actual text
                if (!string.IsNullOrEmpty(q.CorrectAnswer))
                {
                    q.CorrectAnswer = q.CorrectAnswer.ToUpper().Trim();

                    q.CorrectAnswer = q.CorrectAnswer switch
                    {
                        "A" => q.OptionA,
                        "B" => q.OptionB,
                        "C" => q.OptionC,
                        "D" => q.OptionD,
                        _ => q.CorrectAnswer // when model returns full text already
                    };
                }

                q.DateCreated = DateTime.UtcNow;
                q.QuestionText = q.QuestionText?.Trim();
                q.DidYouKnow = q.DidYouKnow?.Trim();
                q.Topic = q.Topic?.Trim();
                q.Difficulty = q.Difficulty?.Trim();
            }


            try
            {
                // Save to DB ---
                _db.QuestionsTable.AddRange(aiQuestions);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) 
            { }

            return aiQuestions;


        }

        private async Task<string> Call_Locally_OllamaAsync(string prompt)
        {

            var url = _config["AISettings:OllamaUrl"];
            var response = await _httpClient.PostAsJsonAsync(url, new
            {
                model = "phi3", // Lite version Using Now later we can change  to llama3
                prompt = prompt
            });

            if (!response.IsSuccessStatusCode)
                throw new Exception("Ollama AI model failed to generate questions.");

            //var result = await response.Content.ReadAsStringAsync();
            return await response.Content.ReadAsStringAsync();

        }

        private async Task<string> Call_HuggingFace_API_Async(string prompt)
        {
            var url = _config["AISettings:HuggingFaceUrl"];

            //var token = _config["AISettings:HuggingFaceApiKey"]?? hfToken;
            var token = hfToken ?? _config["AISettings:HuggingFaceToken"];

            var model = _config["AISettings:HuggingFaceModel"];



            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("HuggingFace API URL is missing from configuration.");

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("HuggingFace API token is not configured. Set it using user-secrets or environment variables.");

            if (string.IsNullOrWhiteSpace(model))
                throw new Exception("HuggingFace model is not configured.");



            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt } 
                }
            };

            request.Content = JsonContent.Create(body);

            var response = await _httpClient.SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HuggingFace AI model failed to generate questions. Response: {responseContent}");

            return responseContent;

        }


    }
}
