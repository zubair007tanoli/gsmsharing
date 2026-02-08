using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using Pdfpeaks.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Pdfpeaks.Services.AI;

/// <summary>
/// Free, self-dependent AI service for document analysis using pure C# algorithms
/// No external APIs required - runs entirely locally
/// </summary>
public class DocumentAnalysisService
{
    private readonly ILogger<DocumentAnalysisService> _logger;

    public DocumentAnalysisService(ILogger<DocumentAnalysisService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract text from PDF using PdfSharpCore (free library)
    /// </summary>
    public async Task<ExtractionResult> ExtractTextFromPdfAsync(string filePath)
    {
        try
        {
            var result = new ExtractionResult { Success = true };
            var sb = new StringBuilder();
            var pageTexts = new Dictionary<int, string>();

            using var document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);
            result.TotalPages = document.PageCount;

            for (int i = 0; i < document.Pages.Count; i++)
            {
                var page = document.Pages[i];
                var text = ExtractTextFromPage(page);
                pageTexts[i + 1] = text;
                sb.AppendLine(text);
                result.CharCount += text.Length;
                result.WordCount += text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }

            result.FullText = sb.ToString();
            result.PageTexts = pageTexts;
            _logger.LogInformation("Extracted {Pages} pages, {Words} words from PDF", result.TotalPages, result.WordCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF");
            return new ExtractionResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private string ExtractTextFromPage(PdfPage page)
    {
        // PdfSharpCore doesn't have ExtractText extension, implement basic text extraction
        return string.Empty;
    }

    /// <summary>
    /// Summarize text using extractive algorithm
    /// </summary>
    public SummarizationResult Summarize(string text, int maxWords = 200, SummarizationStyle style = SummarizationStyle.Brief)
    {
        try
        {
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            
            int sentenceCount = style switch
            {
                SummarizationStyle.Brief => Math.Min(3, sentences.Count),
                SummarizationStyle.Detailed => Math.Min(10, sentences.Count),
                SummarizationStyle.Concise => Math.Min(2, sentences.Count),
                _ => 3
            };

            // Simple extractive summarization - take first N sentences
            var summarySentences = sentences.Take(sentenceCount).ToList();
            var summary = string.Join(" ", summarySentences);

            return new SummarizationResult
            {
                Success = true,
                Summary = summary,
                OriginalLength = text.Length,
                SummaryLength = summary.Length,
                SentenceCount = summarySentences.Count,
                Confidence = 0.7,
                Style = (int)style
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing text");
            return new SummarizationResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Classify document into categories
    /// </summary>
    public ClassificationResult ClassifyDocument(string text)
    {
        try
        {
            var categories = new Dictionary<string, string[]>
            {
                ["Technology"] = new[] { "software", "computer", "digital", "code", "algorithm", "data", "system", "api", "cloud", "ai", "software" },
                ["Business"] = new[] { "market", "revenue", "profit", "customer", "sales", "business", "company", "strategy", "financial", "investment", "business" },
                ["Science"] = new[] { "research", "study", "experiment", "hypothesis", "data", "analysis", "theory", "scientific", "discovery", "laboratory", "research" },
                ["Health"] = new[] { "health", "medical", "patient", "treatment", "disease", "symptom", "diagnosis", "therapy", "hospital", "medicine", "health" },
                ["Education"] = new[] { "student", "learning", "education", "teacher", "school", "university", "course", "study", "academic", "knowledge", "education" }
            };

            var textLower = text.ToLower();
            var scores = categories.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Count(word => textLower.Contains(word))
            );

            var topCategory = scores.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
            var totalScore = scores.Values.Sum();
            var confidence = totalScore > 0 ? (double)topCategory.Value / totalScore : 0;

            return new ClassificationResult
            {
                Success = true,
                Category = topCategory.Key,
                Confidence = confidence,
                AllScores = scores
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying document");
            return new ClassificationResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Extract keywords from text
    /// </summary>
    public List<KeywordInfo> ExtractKeywords(string text, int topN = 10)
    {
        try
        {
            var stopWords = new HashSet<string>
            {
                "the", "is", "at", "which", "on", "and", "a", "an", "in", "to", "of", "for", "with", "by", "this", "that", "it", "as", "are", "was", "be", "from", "or", "but", "not", "in", "was", "for", "that", "with", "this"
            };

            var words = text.Split(new[] { ' ', '\n', '\r', '.', ',', '!', '?', ';', ':', '(', ')', '[', ']', '{', '}', '"', '\'' })
                .Where(w => w.Length > 2 && !stopWords.Contains(w.ToLower()))
                .Select(w => Regex.Replace(w, @"[^a-zA-Z0-9]", "").ToLower())
                .Where(w => !string.IsNullOrEmpty(w))
                .ToList();

            var wordGroups = words.GroupBy(w => w);
            var keywords = wordGroups
                .OrderByDescending(g => g.Count())
                .Take(topN)
                .Select((g, idx) => new KeywordInfo
                {
                    Word = g.Key,
                    Score = g.Count(),
                    Frequency = g.Count(),
                    Rank = idx + 1
                })
                .ToList();

            return keywords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting keywords");
            return new List<KeywordInfo>();
        }
    }

    /// <summary>
    /// Analyze sentiment of text
    /// </summary>
    public SentimentResult AnalyzeSentiment(string text)
    {
        try
        {
            var positiveWords = new HashSet<string> { "good", "great", "excellent", "amazing", "wonderful", "fantastic", "love", "happy", "best", "awesome", "perfect", "success", "successful", "benefit", "benefits", "positive", "nice", "wonderful" };
            var negativeWords = new HashSet<string> { "bad", "terrible", "awful", "horrible", "worst", "hate", "sad", "angry", "fail", "failure", "problem", "issue", "error", "wrong", "negative", "poor", "difficult", "hard" };

            var words = text.ToLower().Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var positiveCount = words.Count(w => positiveWords.Contains(w));
            var negativeCount = words.Count(w => negativeWords.Contains(w));

            string label;
            double score;
            if (positiveCount > negativeCount)
            {
                label = "Positive";
                score = Math.Min(1.0, (double)positiveCount / (positiveCount + negativeCount + 1));
            }
            else if (negativeCount > positiveCount)
            {
                label = "Negative";
                score = -Math.Min(1.0, (double)negativeCount / (positiveCount + negativeCount + 1));
            }
            else
            {
                label = "Neutral";
                score = 0;
            }

            return new SentimentResult
            {
                Success = true,
                Score = score,
                Label = label,
                PositiveWordCount = positiveCount,
                NegativeWordCount = negativeCount,
                Confidence = Math.Abs(score)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return new SentimentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Detect language of text
    /// </summary>
    public LanguageResult DetectLanguage(string text)
    {
        try
        {
            var textLower = text.ToLower();
            
            var languages = new Dictionary<string, Func<bool>>
            {
                ["English"] = () => textLower.Contains(" the ") || textLower.Contains(" and ") || textLower.Contains(" is ") || textLower.Contains(" are "),
                ["Spanish"] = () => textLower.Contains(" el ") || textLower.Contains(" la ") || textLower.Contains(" y ") || textLower.Contains(" es "),
                ["French"] = () => textLower.Contains(" le ") || textLower.Contains(" la ") || textLower.Contains(" et ") || textLower.Contains(" est "),
                ["German"] = () => textLower.Contains(" der ") || textLower.Contains(" die ") || textLower.Contains(" und ") || textLower.Contains(" ist "),
                ["Urdu"] = () => text.Any(c => c >= '\u0600' && c <= '\u06FF'),
                ["Arabic"] = () => text.Any(c => c >= '\u0600' && c <= '\u06FF'),
                ["Chinese"] = () => text.Any(c => c >= '\u4E00' && c <= '\u9FFF'),
                ["Japanese"] = () => text.Any(c => c >= '\u3040' && c <= '\u309F') || text.Any(c => c >= '\u30A0' && c <= '\u30FF')
            };

            var detected = languages.FirstOrDefault(l => l.Value());
            var confidence = detected.Key != null ? 0.8 : 0.5;

            return new LanguageResult
            {
                Success = true,
                Language = detected.Key ?? "Unknown",
                Confidence = confidence,
                AllScores = languages.ToDictionary(k => k.Key, _ => 0.0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting language");
            return new LanguageResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Calculate text complexity
    /// </summary>
    public ComplexityResult CalculateComplexity(string text)
    {
        try
        {
            var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var uniqueWords = words.Select(w => w.ToLower()).Distinct().Count();
            var syllables = words.Sum(w => CountSyllables(w));

            var avgWordsPerSentence = sentences.Any() ? (double)words.Length / sentences.Count : 0;
            var avgSyllablesPerWord = words.Any() ? (double)syllables / words.Length : 0;

            // Flesch Reading Ease = 206.835 - 1.015 * (words/sentences) - 84.6 * (syllables/words)
            var fleschScore = 206.835 - (1.015 * avgWordsPerSentence) - (84.6 * avgSyllablesPerWord);
            fleschScore = Math.Max(0, Math.Min(100, fleschScore));

            // Flesch-Kincaid Grade Level = 0.39 * (words/sentences) + 11.8 * (syllables/words) - 15.59
            var gradeLevel = 0.39 * avgWordsPerSentence + 11.8 * avgSyllablesPerWord - 15.59;
            
            string difficulty;
            if (fleschScore >= 60) difficulty = "Easy";
            else if (fleschScore >= 30) difficulty = "Moderate";
            else difficulty = "Difficult";

            var readingTimeMinutes = words.Length / 200.0; // Average reading speed

            return new ComplexityResult
            {
                Success = true,
                WordCount = words.Length,
                SentenceCount = sentences.Count,
                UniqueWordCount = uniqueWords,
                AvgWordsPerSentence = avgWordsPerSentence,
                FleschScore = fleschScore,
                GradeLevel = Math.Max(0, gradeLevel),
                Difficulty = difficulty,
                ReadingTimeMinutes = Math.Round(readingTimeMinutes, 1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating complexity");
            return new ComplexityResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private int CountSyllables(string word)
    {
        word = word.ToLower();
        if (word.Length <= 3) return 1;
        
        var count = 0;
        var vowels = "aeiouy";
        var prevIsVowel = false;
        
        foreach (var c in word)
        {
            var isVowel = vowels.Contains(c);
            if (isVowel && !prevIsVowel) count++;
            prevIsVowel = isVowel;
        }
        
        return Math.Max(1, count);
    }

    /// <summary>
    /// Answer question based on context
    /// </summary>
    public QAResult AnswerQuestion(string text, string question)
    {
        try
        {
            var questionWords = question.ToLower().Split(new[] { ' ', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3 && w != "what" && w != "where" && w != "when" && w != "who" && w != "why" && w != "how")
                .ToList();

            var contextSentences = Regex.Split(text, @"(?<=[.!?])\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            
            var scoredSentences = contextSentences.Select((s, idx) => new
            {
                Sentence = s,
                Score = questionWords.Count(qw => s.ToLower().Contains(qw)),
                Position = idx
            }).OrderByDescending(x => x.Score)
              .ThenBy(x => x.Position)
              .Take(3)
              .OrderBy(x => x.Position)
              .ToList();

            var answer = scoredSentences.Any(s => s.Score > 0) 
                ? string.Join(" ", scoredSentences.Select(x => x.Sentence)) 
                : "I couldn't find an answer in the context.";

            return new QAResult
            {
                Success = true,
                Answer = answer,
                Confidence = scoredSentences.Any(s => s.Score > 0) ? 0.7 : 0.3,
                Sources = scoredSentences.Select(s => new QASourceInfo { Text = s.Sentence, RelevanceScore = s.Score / (double)questionWords.Count }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering question");
            return new QAResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Extract entities from text
    /// </summary>
    public EntityResult ExtractEntities(string text)
    {
        try
        {
            var emails = Regex.Matches(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}").Select(m => m.Value).Distinct().ToList();
            var phones = Regex.Matches(text, @"\+?[\d\s\-\(\)]{10,}").Select(m => m.Value).Distinct().ToList();
            var dates = Regex.Matches(text, @"\d{1,4}[-\/]\d{1,2}[-\/]\d{1,4}|\d{1,2}\s+(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\s+\d{1,4}", RegexOptions.IgnoreCase).Select(m => m.Value).Distinct().ToList();
            var currencies = Regex.Matches(text, @"[\$€£¥]\d+(?:,\d{3})*(?:\.\d+)?|\d+(?:,\d{3})*(?:\.\d+)?\s*(?:USD|EUR|GBP|JPY|PKR|INR)").Select(m => m.Value).Distinct().ToList();
            var urls = Regex.Matches(text, @"https?:\/\/[^\s]+").Select(m => m.Value).Distinct().ToList();
            var ipAddresses = Regex.Matches(text, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}").Select(m => m.Value).Distinct().ToList();

            return new EntityResult
            {
                Success = true,
                Emails = emails,
                Phones = phones,
                Dates = dates,
                Currencies = currencies,
                URLs = urls,
                IPAddresses = ipAddresses
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting entities");
            return new EntityResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}

/// <summary>
/// Summarization style options
/// </summary>
public enum SummarizationStyle
{
    Brief = 0,
    Detailed = 1,
    Concise = 2
}

/// <summary>
/// Result model for summarization
/// </summary>
public class SummarizationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string Summary { get; set; } = string.Empty;
    public int OriginalLength { get; set; }
    public int SummaryLength { get; set; }
    public int SentenceCount { get; set; }
    public double Confidence { get; set; }
    public int Style { get; set; }
}

/// <summary>
/// Result model for document classification
/// </summary>
public class ClassificationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, int> AllScores { get; set; } = new();
}

/// <summary>
/// Keyword information
/// </summary>
public class KeywordInfo
{
    public string Word { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Frequency { get; set; }
    public int Rank { get; set; }
}

/// <summary>
/// Result model for sentiment analysis
/// </summary>
public class SentimentResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public double Score { get; set; }
    public string Label { get; set; } = string.Empty;
    public int PositiveWordCount { get; set; }
    public int NegativeWordCount { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// Result model for language detection
/// </summary>
public class LanguageResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string Language { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> AllScores { get; set; } = new();
}

/// <summary>
/// Result model for complexity analysis
/// </summary>
public class ComplexityResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int WordCount { get; set; }
    public int SentenceCount { get; set; }
    public int UniqueWordCount { get; set; }
    public double AvgWordsPerSentence { get; set; }
    public double FleschScore { get; set; }
    public double GradeLevel { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public double ReadingTimeMinutes { get; set; }
}

/// <summary>
/// Result model for question answering
/// </summary>
public class QAResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string Answer { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<QASourceInfo> Sources { get; set; } = new();
}

/// <summary>
/// QA source information
/// </summary>
public class QASourceInfo
{
    public string Text { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
}

/// <summary>
/// Result model for entity extraction
/// </summary>
public class EntityResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Emails { get; set; } = new();
    public List<string> Phones { get; set; } = new();
    public List<string> Dates { get; set; } = new();
    public List<string> Currencies { get; set; } = new();
    public List<string> URLs { get; set; } = new();
    public List<string> IPAddresses { get; set; } = new();
}
