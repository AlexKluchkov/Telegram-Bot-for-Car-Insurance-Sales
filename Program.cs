using System;
using Mindee;
using Mindee.Http;
using Mindee.Input;
using Mindee.Product.Generated;
using Mindee.Product.Passport;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using var cts = new CancellationTokenSource();
var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
var bot = new TelegramBotClient(token);
var apiKey = Environment.GetEnvironmentVariable("MINDEE_TOKEN");
int step = 0;

string InsuranceAgreementText = "";
string PassportName = "";
string VehicleCardRegistrationDate = "";
string VehicleCardVhicleColor = "";
string VehicleCardVehicleMake = "";

string Insurance_Policy_Template_Path = Path.Combine(AppContext.BaseDirectory, "Auto_Insurance_Policy_Template.txt");
string Insurance_Policy_Path = "Auto_Insurance_Policy.txt";

var me = await bot.GetMe();

bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

await Task.Delay(-1);

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel();

Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception); 
    return Task.CompletedTask;
}

async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        await bot.SendMessage(chatId: msg.Chat.Id, text: $"Hello {msg.Chat.FirstName}! My name is {me.FirstName}!");
        await Task.Delay(1000);
        await bot.SendMessage(chatId: msg.Chat.Id, text: $"Please, send me a photo of your passport.");
        step = 1;
        InsuranceAgreementText = File.ReadAllText(Insurance_Policy_Template_Path);
    }
    else if (msg.Photo != null && msg.Photo.Length > 0)
    {
        var photo = msg.Photo.Last();
        var file = await bot.GetFile(photo.FileId);
        var filePath = file.FilePath;
        string TmpPhoto = "photo.jpg";
        using (var saveImageStream = new FileStream(TmpPhoto, FileMode.Create))
        {
            await bot.DownloadFile(filePath!, saveImageStream);
        }
        if (step == 1)
        {
            await PhotoPassportAnalysis(msg.Chat, TmpPhoto);
        }
        else if(step == 2)
        {
            await PhotoVehicleCardAnalysis(msg.Chat, TmpPhoto);
        }
        if (File.Exists(TmpPhoto))
        {
            File.Delete(TmpPhoto); //Delete temporary file
        }
    }
    else if (msg.Text != null)  //AI
    {
        TelegramBotAI botAI = new TelegramBotAI();
        var reply = await botAI.GetAiResponseAsync(msg.Text);
        await bot.SendMessage(chatId: msg.Chat.Id, text: reply ?? "Unable to get response from AI");
    }
}

async Task PhotoPassportAnalysis(ChatId id, string TmpPhoto) 
{
    // Construct a new client
    MindeeClient mindeeClient = new MindeeClient(apiKey);

    var inputSource = new LocalInputSource(TmpPhoto);
    // Call the product asynchronously with auto-polling
    var response = await mindeeClient.ParseAsync<PassportV1>(inputSource);

    // Print a summary of all the predictions
    var ResultPassportCard = response.Document.Inference.Prediction;

    await bot.SendMessage(chatId: id, text: $"Given Name:{ResultPassportCard.GivenNames[0]}\n" +
        $"Date of Birth: {ResultPassportCard.BirthDate.Value}\n" +
        $"Place of Birth: {ResultPassportCard.BirthPlace.Value}\n" +
        $"Country Code: {ResultPassportCard.Country.Value}\n" +
        $"Expiry Date: {ResultPassportCard.ExpiryDate.Value}\n" +
        $"Gender: {ResultPassportCard.Gender.Value}");
    PassportName = ResultPassportCard.GivenNames[0].ToString();

    await bot.SendMessage(id, "Is the data provided correctly?",
            replyMarkup: new InlineKeyboardButton[] { "Yes", "No" });
}

async Task PhotoVehicleCardAnalysis(ChatId id, string TmpPhoto)
{
    //When analyzing the vehicle's Card, was used Custom API
    MindeeClient mindeeClient = new MindeeClient(apiKey);

    // Load an input source as a path string
    // Other input types can be used, as mentioned in the docs
    var inputSource = new LocalInputSource(TmpPhoto);

    // Set the endpoint configuration
    CustomEndpoint endpoint = new CustomEndpoint(
        endpointName: "vehicleregistrationv3",
        accountName: "Alexander1224",
        version: "1"
    );

    // Call the product asynchronously with auto-polling
    var response = await mindeeClient
        .EnqueueAndParseAsync<GeneratedV1>(inputSource, endpoint);

    var ResultVehicleCard = response.Document.Inference.Prediction;

    // Print a summary of all the predictions
    var pred = response.Document.Inference.Prediction;
    //When working with Custom API, I can`t split the report into fields.
    foreach (var field in response.Document.Inference.Prediction.Fields)
    {
        await bot.SendMessage(chatId: id, $"{field.Key}: {field.Value}");
        if(field.Key == "registration_date")
        {
            VehicleCardRegistrationDate = field.Value.ToString();
        }
        else if(field.Key == "vehicle_color")
        {
            VehicleCardVhicleColor = field.Value.ToString();
        }
        else if (field.Key == "vehicle_make")
        {
            VehicleCardVehicleMake = field.Value.ToString();
        }
    }

    await bot.SendMessage(id, "Is the data provided correctly?",
            replyMarkup: new InlineKeyboardButton[] { "Yes", "No" });
}

async Task PriceQuotation(ChatId id)
{
    await bot.SendMessage(id, "The fixed price for insurance is 100 USD. Do you agree with the price?",
            replyMarkup: new InlineKeyboardButton[] { "Yes", "No" });
}

async Task Insurance_Policy_Issuance(ChatId id)
{
    await using (StreamWriter writer = new StreamWriter(Insurance_Policy_Path))
    {
        writer.WriteLine(InsuranceAgreementText);
    }    
    using var fileStream = File.OpenRead(Insurance_Policy_Path);
    await bot.SendDocument(
        chatId: id,
        document: fileStream
    );
}


async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
    {
        if ((query.Data == "Yes") && (step == 1))
        {
            await bot.SendMessage(chatId: query.Message!.Chat.Id, text: $"Please send a photo of your driver's license.");
            step = 2;
            InsuranceAgreementText = InsuranceAgreementText.Replace("[Name of insured person]", PassportName);
        }
        else if((query.Data == "No")&& (step == 1))
        {
            await bot.SendMessage(chatId: query.Message!.Chat.Id, text: $"Send your passport photo again.");
        }
        else if ((query.Data == "Yes") && (step == 2))
        {
            await PriceQuotation(query.Message!.Chat);
            step = 3;
            InsuranceAgreementText = InsuranceAgreementText.Replace("[registration date]", VehicleCardRegistrationDate);
            InsuranceAgreementText = InsuranceAgreementText.Replace("[vehicle color]", VehicleCardVhicleColor);
            InsuranceAgreementText = InsuranceAgreementText.Replace("[e.g., Toyota Camry]", VehicleCardVehicleMake);
        }
        else if ((query.Data == "No") && (step == 2))
        {
            await bot.SendMessage(chatId: query.Message!.Chat.Id, text: $"Send a photo of your vehicle card again");
        }
        else if((query.Data == "Yes")&& (step == 3))
        {
            InsuranceAgreementText = InsuranceAgreementText.Replace("[Insured Amount]", "100 USD");
            await Insurance_Policy_Issuance(query.Message!.Chat);
        }
        else if ((query.Data == "No") && (step == 3))
        {
            await bot.SendMessage(chatId: query.Message!.Chat.Id, text: $"Unfortunately, 100 USD is the only affordable price.");
        }
    }
}
