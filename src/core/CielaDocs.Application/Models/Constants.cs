namespace CielaDocs.Application.Models
{
    public class Constants
    {
        public static string[] arr = { " ", "Съгласен", "НЕсъгласен", "За доработка", "Прекратявам процедурата" };
        public static int Onr = 0;

        public const string
    Incoming_NoResponceFormat =
         "<div style=' padding: 22px; font-size: 13px;font-weight: bold; padding-bottom: 15px; color: #8C8C8C;'>" +
             "<div style='font-size: 13px;font-weight: bold;padding-bottom: 15px;'>{0}</div>" +
             "<div style='color: #8C8C8C;padding-bottom: 22px;'>" +
                 "<div>Вх. N:{1}/{2} Подател:{3} Адрес:{4}</div>" +
                 "<div>Вид документ:<b>{5}</b> Тип: <font color='red'>{6}</font></div>" +
                 "<div>До: {7}</div>" +
                 "<div>Относно: {8}</div>" +
                 "<div>Срок за изпълнение: {6}</div>" +
                 "<div>{7}</div>" +
             "</div>" +
             "<div style='height: 6px;background: #DEDEDE; height: 1px;'></div>" +
             "<div class='Body'>{8}</div>" +
             "<div>Статус на искането: <font color='red'> {9}</font></div>" +
         "</div>";

        public const string ReplyMessageFormat =
         "<div style=' padding: 22px; font-size: 13px;font-weight: bold; padding-bottom: 15px; color: #8C8C8C;'>" +
             "<div style='font-size: 13px;font-weight: bold;padding-bottom: 15px;'>{0}</div>" +
             "<div style='color: #8C8C8C;padding-bottom: 22px;'>" +
                 "<div>Отговор на Искане N:{1}/{2}</div>" +
                 "<div>От Изпълнителя: {3}</div>" +
                 "<div>До Заявителя: {4}</div>" +
                 "<div>Отговор на: {5}</div>" +
                 "<div>Срок за изпълнение: {6}</div>" +
                 "<div>{7}</div>" +
             "</div>" +
             "<div style='height: 6px;background: #DEDEDE; height: 1px;'></div>" +
             "<div class='Body'>{8}</div>" +
             "<div>Статус на искането: <font color='red'> {9}</font></div>" +
         "</div>";
        public const string BackToSenderMessageFormat =
            "<div style=' padding: 22px; font-size: 13px;font-weight: bold; padding-bottom: 15px; color: #8C8C8C;'>" +
                "<div style='font-size: 13px;font-weight: bold;padding-bottom: 15px;'>{0}</div>" +
                "<div style='color: #8C8C8C;padding-bottom: 22px;'>" +
                    "<div>Върнато до заявителя за съгласуване по Искане N:{1}/{2}</div>" +
                    "<div>От Изпълнителя: {3}</div>" +
                    "<div>До Заявителя: {4}</div>" +
                    "<div>Отговор на: {5}</div>" +
                    "<div>Срок за изпълнение: {6}</div>" +
                    "<div>{7}</div>" +
                "</div>" +
                "<div style='height: 6px;background: #DEDEDE; height: 1px;'></div>" +
                "<div class='Body'>{8}</div>" +
                "<div>Статус на искането:<font color='red'> {9}</font></div>" +
            "</div>";
        public const string ReSentToSenderMessageFormat =
            "<div style=' padding: 22px; font-size: 13px;font-weight: bold; padding-bottom: 15px; color: #8C8C8C;'>" +
                "<div style='font-size: 13px;font-weight: bold;padding-bottom: 15px;'>{0}</div>" +
                "<div style='color: #8C8C8C;padding-bottom: 22px;'>" +
                    "<div>Препратено до изпълнителя след съгласуване по Искане N:{1}/{2}</div>" +
                    "<div>От Заявителя: {3}</div>" +
                    "<div>До Изпълнителя: {4}</div>" +
                    "<div>Отговор на: {5}</div>" +
                    "<div>Срок за изпълнение: {6}</div>" +
                    "<div>{7}</div>" +
                "</div>" +
                "<div style='height: 6px;background: #DEDEDE; height: 1px;'></div>" +
                "<div class='Body'>{8}</div>" +
                "<div>Статус на искането:<font color='red'> {9}</font></div>" +
            "</div>";
        public const string CaseTaskFormat =
     "<div style=' padding: 22px; font-size: 13px;font-weight: bold; padding-bottom: 15px; color: #8C8C8C;'>" +
         "<div style='font-size: 13px;font-weight: bold;padding-bottom: 15px;'>{0}</div>" +
         "<div style='color: #8C8C8C;padding-bottom: 22px;'>" +
             "<div>Вид: {1}</div>" +
             "<div>От: {2}</div>" +
             "<div>До: {3}</div>" +
             "<div>Описание: {4}</div>" +
             "<div>{5}</div>" +
         "</div>" +
         "<div style='height: 6px;background: #DEDEDE; height: 1px;'></div>" +
         "<div class='Body'>{6}</div>" +
         "<div>Статус на отговора: <font color='red'>{7}</font></div>" +
     "</div>";
        public const string ReplyCaseTaskFormat =
        "<div style=' padding: 22px; font-size: 13px;font-weight: normal; padding-bottom: 15px; color: #8C8C8C;'>" +
            "<div style='font-size: 13px;font-weight: normal;padding-bottom: 15px;'>{0}</div>" +
            "<div style='color: #8C8C8C;padding-bottom: 22px;'>" +
                "<div>Отговор на Искане N:{1}/{2}</div>" +
                "<div>От Изпълнителя: {3}</div>" +
                "<div>До Заявителя: {4}</div>" +
                "<div>Отговор на: {5}</div>" +
                "<div>Срок за изпълнение: {6}</div>" +
                "<div>{7}</div>" +
            "</div>" +
            "<div style='height: 6px;background: #DEDEDE; height: 1px;'></div>" +
            "<div class='Body'>{8}</div>" +
            "<div>Статус на отговора: <font color='red'> {9}</font></div>" +
        "</div>";
        public const string PreviewMsgMessageFormat =
        "<div style=' padding: 10px; font-size: 13px;font-weight: normal; padding-bottom: 5px; color: #8C8C8C;'>" +
            "<div style='font-size: 13px;font-weight: normal;padding-bottom: 5px;'><font color='blue'>Относно:</font>{0}</div>" +
            "<div style='color: #8C8C8C;padding-bottom: 5px;'>" +
                "<div><font color='blue'>Вид съобщение:</font>{1}, <font color='blue'>Док N:</font>{2}/{3}, От: {4}, До: {5} Дaта: {6}</div>" +
                "<div>{7}</div>" +
            "</div>" +
            "<div style='height: 1px;background: #DEDEDE; height: 1px;'></div>" +
            "<div class='Body'><font color='blue'>Забележка:</font>{8}, Статус:<font color='red'>{9}</font></div>" +
        "</div>";
        public const string PreviewTaskMessageFormat =
      "<div style=' padding: 10px; font-size: 13px;font-weight:normal; padding-bottom: 5px; color: #8C8C8C;'>" +
          "<div style='font-size: 13px;font-weight: normal;padding-bottom: 5px;'><font color='blue'>Относно:</font>{0}</div>" +
          "<div style='color: #8C8C8C;padding-bottom: 5px;'>" +
            "<div><font color='blue'>Вид съобщение:</font>{1}, <font color='blue'>Док N:</font>{2}/{3}, От: {4}, До: {5} Дaта: {6}</div>" +
              "<div>Срок за изпълнение: {7}</div>" +
              "<div>{8}</div>" +
          "</div>" +
          "<div style='height: 1px;background: #DEDEDE; height: 1px;'></div>" +
           "<div class='Body'><font color='blue'>Забележка:</font>{9}, Статус:<font color='red'>{10}</font></div>" +
      "</div>";
        public const string PreviewCardTmpFormat =
    "<div style=' padding: 10px; font-size: 13px;font-weight: normal; padding-bottom: 5px; color: #8C8C8C;'>" +
        "<div style='font-size: 13px;font-weight: normal;padding-bottom: 5px;'>{0}</div>" +
        "<div style='color: #8C8C8C;padding-bottom: 5px;'>" +
           "<div>Тип:{1}, Вид документ:{2},Работен Док N:{3}/{4},Изготвил: {5},Автор: {6}</div>" +
        "</div>" +
        "<div style='height: 1px;background: #DEDEDE; height: 1px;'></div>" +
        "<div class='Body'>{7}</div>" +
        "<div>Деловоден номер: <font color='red'>{8}/{9}</font></div>" +
    "</div>";
    }
}
