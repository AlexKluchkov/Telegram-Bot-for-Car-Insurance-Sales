# Telegram-Bot-for-Car-Insurance-Sales

Telegram bot name: @InsuranceSalesForCarTelegramBot

  A Telegram bot that assists users in purchasing car insurance by processing
user-submitted documents, interacting through AI-driven communications, and confirming
transaction details.The following libraries were used in the program: Mindee, Telegram.Bot.
  The OnMessage method is called when the user sends a message to the telegram bot. The method can handle the command /start and also images. When working with images, program are executed methods PhotoPassportAnalysis and PhotoVehicleCardAnalysis. The PhotoPassportAnalysis method uses the PassportV1 model, and the PhotoVehicleCardAnalysis method uses a custom model. When working with Custom API, I can`t split the report into fields. So I tried to use foreach to split the data.
  The variable "step" determines at what stage the telegram bot working. 1 - when the program works with a passport photo, 2 - when the program works with a Vehicle Card photo, 3 - when the program asks the user about price. The Insurance_Policy_Issuance creates an Insurance Policy based on the template and then sends the Insurance Policy to the user.
