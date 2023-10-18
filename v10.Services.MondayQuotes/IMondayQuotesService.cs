using v10.Services.MondayQuotes.Enums;

namespace v10.Services.MondayQuotes;

public interface IMondayQuotesService
{
    Task<string> GetQuote(QuoteCategory category = QuoteCategory.Funny);
}
