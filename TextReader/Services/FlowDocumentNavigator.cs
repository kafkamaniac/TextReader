using System.Windows.Documents;

namespace TextReader.Services;

public static class FlowDocumentNavigator
{
    public static TextPointer? GetTextPointerAtOffset(
        TextPointer start, //Начальная позиция
        int targetOffset) //нулевое смещение в документе
    {
        int currentOffset = 0; //начинаем смещение с нуля
        TextPointer? navigator = start; //идём по документу начиная с начальной позиции

        while (navigator != null) //пока не дошли до конца документа
        {
            if (IsTextRun(navigator)) //если текущая позиция указывает на текстовый фрагмент
            {
                TextPointer? result = FindPointerInRun( //ищем указатель в текущем фрагменте текста
                    navigator, //начальная позиция фрагмента
                    currentOffset, //текущее смещение в документе
                    targetOffset); //целевое смещение в документе

                if (result != null)
                    return result;

                currentOffset += GetRunLength(navigator); //увеличиваем текущее смещение
                                                          //на длину текущего фрагмента текста
                navigator = MoveToNextRun(navigator); //переходим к следующему фрагменту текста
            }
            else
            {
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
                //Если текущая позиция не указывает на текстовый фрагмент,
                //то просто переходим к следующей позиции в документе
            }
        }

        return null;
    }

    private static bool IsTextRun(TextPointer pointer)
    {
        return pointer.GetPointerContext(LogicalDirection.Forward)
               == TextPointerContext.Text;
    }

    private static int GetRunLength(TextPointer pointer)
    {
        return pointer.GetTextInRun(LogicalDirection.Forward).Length;
    }

    private static TextPointer MoveToNextRun(TextPointer pointer)
    {
        return pointer.GetPositionAtOffset(GetRunLength(pointer))!;
    }

    private static TextPointer? FindPointerInRun(
        TextPointer runStart,
        int currentOffset,
        int targetOffset)
    {
        int runLength = GetRunLength(runStart);

        if (targetOffset > currentOffset + runLength)
            return null;

        int offsetInsideRun = targetOffset - currentOffset;

        return runStart.GetPositionAtOffset(offsetInsideRun);
    }
}