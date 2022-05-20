using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace MLTA_1_3
{
    /// <summary>
    /// Таблица истинности
    /// </summary>
    public partial class Result : Page
    {
        private readonly List<char> lettersList;
        private string infixNotation;
        /// <summary>
        /// Таблица истинности
        /// </summary>
        /// <param name="infixNotation">Инфиксная запись</param>
        public Result(string infixNotation)
        {
            InitializeComponent();
            lettersList = new List<char>();
            switch (CheckInfix(infixNotation))
            {
                case true:
                    break;
                case false:
                    MessageBox.Show("Инфиксная запись введена неправильно!");
                    return;
            }
            this.infixNotation = ToPostfix(infixNotation);
            foreach(var symbol in infixNotation)
            {
                if (char.IsLetter(symbol))
                {
                    lettersList.Add(symbol);
                    letters.Content += $"{symbol}";
                    function.Content += $"{symbol}";
                }
            }
            function.Content += ")";
            Run();
        }
        /// <summary>
        /// Построение таблицы истинности по инфиксной записи
        /// </summary>
        public void Run()
        {
            for (int i = 0; i < (int)Math.Pow(2, lettersList.Count); i++)
            {
                Dictionary<char, bool> currentRow;
                Label letters = new Label();
                try
                {
                    currentRow = CreateRow(lettersList, i);
                    foreach (var symbol in currentRow)
                    {
                        letters.Content += symbol.Value ? "1" : "0";
                    }
                }
                catch
                {
                    return;
                }
                Label result = new Label();
                result.Content = FunctionResult(currentRow) ? "1" : "0";
                result.FontSize = 16;
                result.Foreground = Brushes.AliceBlue;
                Function.Children.Add(result);
                letters.FontSize = 16;
                letters.Foreground = Brushes.AliceBlue;
                Letters.Children.Add(letters);
            }
        }
        /// <summary>
        /// Создаёт строку по таблице истинности
        /// </summary>
        /// <param name="num">Номер строки</param>
        /// <returns>Массив <see cref="bool[]"/></returns>
        private Dictionary<char, bool> CreateRow(List<char> letters, int num)
        {
            Dictionary<char, bool> result = new Dictionary<char, bool>();
            foreach (var symbol in letters)
            {
                try
                {
                    result.Add(symbol, false);
                }
                catch
                {
                    MessageBox.Show("Неправильно введена инфиксная запись:\nЭта переменная уже используется!");
                    return null;
                }
            }
            int i = result.Count - 1;
            while (num != 0)
            {
                result[letters[i]] = num % 2 != 0;
                i--;
                if (i < 0)
                {
                    return result;
                }
                num /= 2;
            }
            return result;
        }
        /// <summary>
        /// Результат функции при входных параметрах
        /// </summary>
        /// <param name="currentRow">Входные параметры</param>
        /// <returns>Булевое значение</returns>
        private bool FunctionResult(Dictionary<char, bool> currentRow)
        {
            Stack<bool> stack = new Stack<bool>();
            foreach (var symbol in infixNotation)
            {
                switch (char.IsLetter(symbol))
                {
                    case true:
                        stack.Push(currentRow[symbol]);
                        break;
                    case false:
                        switch (symbol)
                        {
                            case '¬':
                                stack.Push(!stack.Pop());
                                break;
                            default:
                                bool op2 = stack.Pop();
                                bool op1 = stack.Pop();
                                switch (symbol)
                                {
                                    case '⊕':
                                        stack.Push((op1 || op2) && (!op1 || !op2));
                                        break;
                                    case '⋀':
                                        stack.Push(op1 & op2);
                                        break;
                                    case '⋁':
                                        stack.Push(op1 || op2);
                                        break;
                                    case '↔':
                                        stack.Push(op1 == op2);
                                        break;
                                    case '→':
                                        stack.Push(!op1 || op2);
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }
            return stack.Peek();
        }
        /// <summary>
        /// Преобразование инфиксной записи в постфиксную
        /// </summary>
        /// <returns>Результат преобразования</returns>
        private static string ToPostfix(string infixNotation)
        {
            string postfix = "";
            Stack<char> stack = new Stack<char>();
            foreach (var symbol in infixNotation)
            {
                switch (char.IsLetter(symbol))
                {
                    case true:
                        postfix += symbol.ToString();
                        if (stack.Count != 0)
                        {
                            while (stack.Peek() == '¬')
                            {
                                postfix += stack.Pop();
                                if (stack.Count == 0)
                                {
                                    break;
                                }
                            }
                        }
                        break;
                    case false:
                        switch (symbol)
                        {
                            case '(':
                                stack.Push(symbol);
                                break;
                            case ')':
                                while (stack.Peek() != '(')
                                {
                                    postfix += stack.Pop();
                                }
                                stack.Pop();
                                if (stack.Count != 0)
                                {
                                    while (stack.Peek() == '¬')
                                    {
                                        postfix += stack.Pop();
                                        if (stack.Count == 0)
                                        {
                                            break;
                                        }
                                    }
                                }
                                break;
                            default:
                                if (stack.Count == 0)
                                {
                                    stack.Push(symbol);
                                }
                                else
                                {
                                    if (Priority(stack.Peek()) > Priority(symbol))
                                    {
                                        postfix += stack.Pop();
                                    }
                                    stack.Push(symbol);
                                }
                                break;
                        }
                        break;
                }
            }
            while (stack.Count != 0)
            {
                postfix += stack.Pop();
            }
            return postfix;
        }
        /// <summary>
        /// Приоритет операции
        /// </summary>
        /// <param name="operation">Операция</param>
        /// <returns>Приоритет</returns>
        private static int Priority(char operation)
        {
            switch (operation)
            {
                case '¬':
                    return 3;
                case '⋀':
                    return 2;
                case '⋁':
                    return 1;
                case '⊕':
                    return 1;
                default:
                    return 0;
            }
        }
        /// <summary>
        /// Проверка на правильность написания инфиксной нотации
        /// </summary>
        /// <param name="infixNotation">Инфиксная запись</param>
        /// <returns>Булевое значение</returns>
        public static bool CheckInfix(string infixNotation)
        {
            char[] operators = { '(', ')', '⋁', '⋀', '→', '↔', '¬', '⊕' };
            try
            {
                if (operators.Contains(infixNotation[^1]) && (infixNotation[^1] != ')') || operators.Contains(infixNotation[0]) && (infixNotation[0] != '¬') && (infixNotation[0] != '('))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            Stack<char> stack = new Stack<char>();
            foreach (var symbol in infixNotation)
            {
                switch (symbol)
                {
                    case '(':
                        stack.Push('(');
                        break;
                    case ')':
                        if (stack.Count == 0)
                        {
                            return false;
                        }
                        else
                        {
                            stack.Pop();
                        }
                        break;
                    default:
                        break;
                }
            }
            if (stack.Count != 0)
            {
                return false;
            }
            for (int i = 0; i < infixNotation.Length - 1; i++)
            {
                char currentSymbol = infixNotation[i];
                char nextSymbol = infixNotation[i + 1];
                if (char.IsLetter(currentSymbol) && (!(operators.Contains(nextSymbol)) || (nextSymbol == '(') || (nextSymbol == '¬')))
                {
                    return false;
                }
                if (operators.Contains(currentSymbol) && (operators.Contains(nextSymbol)) && (nextSymbol != '(') && (nextSymbol != '¬') && (currentSymbol != ')'))
                {
                    return false;
                }
                if ((currentSymbol == ')') && ((char.IsLetter(nextSymbol)) || (nextSymbol == '¬')))
                {
                    return false;
                }
            }
            return true;
        }
    }
}