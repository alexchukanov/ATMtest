using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtmTest
{
    /*
        There is a special cash machine, without any security, that dispenses money(notes and coins). The machine has a given initial state of what coins and notes it has available.
    The initial state is: 100x1p, 100x2p, 100x5p, 100x10p, 100x20p, 100x50p, 100x£1, 100x£2, 50x£5, 50x£10, 50x£20, 50x£50.
    You should create a program that is given the value to withdraw as an input.
    Program the cash machine, so it has 2 algorithms that can be swapped (swapping can be done by rebuilding and rerunning the application):
    1. Algorithm that returns least number of items(coins or notes)
    2. Algorithm that returns the highest number of £20 notes possible
    Output the number of coins and notes given for each withdrawal.
    The machine should output the count and value of coins and notes dispensed and the balance amount left.
    The program should be extendible and written using .NET framework.Use the best approach you can to implement the solution.
    Examples:

    ALGORITHM 1
    Input (Withdrawal amount)
    120.00
    Output
    £50x2, £20x1
    £X.XX balance

    ALGORITHM 2
    Input
    120.00
    Output
    £20x6
    £X.XX balance
    */

    public class Program
    {
        static eTransactionMode transactionMode = eTransactionMode.ALGORITHM_1;
       
        static Atm atm = null;

        static void Main(string[] args)
        {
            int algorithmN = 1;

            /* we use only ALGORITHM_1
            Atm.PrintMessage("Select algorithm number (1 or 2):", false);
            int algorithmN = Atm.ReadNumber();

            if (algorithmN == 0 || algorithmN > 2)
            {
                Atm.PrintMessage("Algorithm number = (1 or 2)", true);
                return;
            }
            */
                       
            Atm.PrintMessage("Enter a desired banknote nominal to withdraw (10 or 20 or 50):", false);
            int noteN = Atm.ReadNumber();

            if (noteN != 10 && noteN != 20 && noteN != 50)
            {
                Atm.PrintMessage("Wrong banknote nominal, press any key to exit.", true);
                return;
            }

            Atm.PrintMessage("Enter amount in £ to withdraw:", false);
            int withdrawInput = Atm.ReadNumber();

            
            transactionMode = (eTransactionMode)Enum.Parse(typeof(eTransactionMode), algorithmN.ToString());
            eNote firstNote =  (eNote)Enum.Parse(typeof(eNote), noteN.ToString());

            atm = new Atm(transactionMode, firstNote);

            double startBalance = atm.GetAtmBalance();

            //header
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n");
            sb.AppendLine(String.Format(transactionMode.ToString()));
            sb.AppendLine(String.Format(@"£{0:0.00} ATM balance", startBalance));
           
            Atm.PrintMessage(sb.ToString(), false);

            if (startBalance < withdrawInput)
            {
                Atm.PrintMessage("Please, try to withdraw a lesser amount", false);
                Atm.PrintMessage("Press any key", true);
                return;
            }

            bool isTrransactionOk = atm.WithdrawMoney(withdrawInput);

            if (isTrransactionOk)
            {
                atm.PrintReceipt();
                Atm.PrintMessage("Transaction is OK, take your receipt and money", false);
                Atm.PrintMessage("Press any key", true);
            }
            else
            {
                Atm.PrintMessage("Transaction is failed, take your card", false);
                Atm.PrintMessage("Press any key", true);
            }
        }
    }

    

    public interface ITransaction
    {
        WithdrawalTransaction CreateTransaction(List<CashDeck> cashDeckList, int entry);
    }

    public class Atm
    {
        ITransaction transactionModeAlgo = null;
        List<CashDeck> cashDeckList = null;
        WithdrawalTransaction WithdrawalTransaction = null;       

        public Atm(eTransactionMode transactionMode, eNote firsNote)
        {
            switch (transactionMode)
            {
                case eTransactionMode.ALGORITHM_1:
                    transactionModeAlgo = new TransactionModeAlgoA();
                    break;
                case eTransactionMode.ALGORITHM_2:
                    transactionModeAlgo = new TransactionModeAlgoA(); // TransactionModeAlgoB() to be implemented
                    break;
                default:
                    transactionModeAlgo = new TransactionModeAlgoA();
                    break;
            }

            switch (firsNote)
            {
                //initial state generation: 100x£1, 100x£2, 50x£5, 50x£10, 50x£20, 50x£50.
                case eNote.GBP50:                   
                    cashDeckList = new List<CashDeck>
                    {                       
                        new CashDeck(eNote.GBP50,50),
                        new CashDeck(eNote.GBP20,50),
                        new CashDeck(eNote.GBP10,50),                        
                    };
                    break;

                case eNote.GBP20:
                    cashDeckList = new List<CashDeck>
                    {                       
                       new CashDeck(eNote.GBP20,50),
                       new CashDeck(eNote.GBP50,50),                        
                       new CashDeck(eNote.GBP10,50),                       
                    };
                    break;

                case eNote.GBP10:
                    cashDeckList = new List<CashDeck>
                    {                       
                       new CashDeck(eNote.GBP10,50),
                       new CashDeck(eNote.GBP50,50),
                       new CashDeck(eNote.GBP20,50),
                    };
                    break;

                default:
                    cashDeckList = new List<CashDeck>
                    {                       
                        new CashDeck(eNote.GBP50,50),
                        new CashDeck(eNote.GBP20,50),
                        new CashDeck(eNote.GBP10,50),
                    };                    
                    break;
            }

                    cashDeckList.Add(new CashDeck(eNote.GBP5, 50));
                    cashDeckList.Add(new CashDeck(eNote.GBP2, 50));
                    cashDeckList.Add(new CashDeck(eNote.GBP1, 50));
        }

        public bool WithdrawMoney(int withdrawAmount)
        {
            bool isWithdrawMoneyOk = false;

            WithdrawalTransaction = transactionModeAlgo.CreateTransaction(cashDeckList, withdrawAmount);

            int transactionAmount = WithdrawalTransaction.GetTransactAmount();
            int atmBalance = GetAtmBalance();

            if (transactionAmount > atmBalance)
            {
                PrintMessage("Transaction is failed, choose another amount", true);
            }
            else
            {
                isWithdrawMoneyOk = PostTransaction();
            }

            return isWithdrawMoneyOk;
        }

        public int GetAtmBalance()
        {
            int atmBalance = 0;

            foreach (CashDeck cashDeck in cashDeckList)
            {
                atmBalance += cashDeck.CashAmount;
            }

            return atmBalance;
        }

        public static void PrintMessage(string message, bool isInputWait)
        {            
            Console.WriteLine(message);
            if (isInputWait)
            {
                Console.ReadKey();
            }
        }

        public static int ReadNumber()
        {
            string amount = Console.ReadLine();

            int amountValue = -1;

            if (!int.TryParse(amount, out amountValue) || amountValue <= 0)
            {
                PrintMessage("Incorrect value", false);
            }
            
            return amountValue;
        }

        public bool PostTransaction()
        {
            bool isResultOk = false;
            int postCount = 0;

            CashDeck[] cashDeckListCopy = new CashDeck[cashDeckList.Count]; 
            cashDeckList.CopyTo(cashDeckListCopy);

            foreach (CashDeck cashDeck in cashDeckListCopy)
            {
                if(cashDeck.RequestedNoteNum != 0 && cashDeck.RequestedNoteNum <= cashDeck.NoteNum)
                {
                    cashDeck.NoteNum -= cashDeck.RequestedNoteNum;
                    cashDeck.RequestedNoteNum = 0;
                    postCount++;
                }
                else
                {
                    continue;
                }
            }

            if (postCount != 0)
            {
                cashDeckList = new List<CashDeck>(cashDeckListCopy);
                isResultOk = true;
            }

            return isResultOk;
        }

        public void PrintReceipt()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Input [Withdrawal amount]");
            sb.AppendLine(WithdrawalTransaction.InputWithdraw.ToString());
            sb.AppendLine("Output");

            foreach (CashSet cashSet in WithdrawalTransaction.CashSetList)
            {
                sb.Append(cashSet.ToString());
            }

            sb.Remove(sb.Length-1, 1);
            sb.AppendLine("");
            sb.AppendLine(String.Format(@"£{0:0.00} ATM balance", GetAtmBalance()));

            PrintMessage(sb.ToString(), false);
        }
    }

    public class CashDeck
    {
        public eNote Note { get; set; }
        public int NoteNum { get; set; }

        public CashDeck(eNote note, int noteNum)
        {
            Note = note;
            NoteNum = noteNum;
        }

        public int CashAmount
        {
            get
            {
                return NoteNum * (int)Note;
            }
        }

        int requestedNoteNum = 0;
        public int RequestedNoteNum
        {
            get
            {
                return requestedNoteNum;
            }
            set
            {
                requestedNoteNum = value;
            }
        }

        public CashSet AskMaxCashAmount(int askedCashAmount)
        {   

            if (NoteNum != 0)
            {
                RequestedNoteNum = askedCashAmount / (int)Note;
            }
            else
            {
                RequestedNoteNum = 0;
            }

            return new CashSet(Note, RequestedNoteNum); 
        }

        public bool WithdrawAmount()
        {
            bool result = false;

            if (NoteNum != 0 && NoteNum < RequestedNoteNum)
            {
                NoteNum -= RequestedNoteNum;
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }
    }

    public class WithdrawalTransaction
    {
        public List<CashSet> CashSetList { get; set; }
        public int InputWithdraw { get; set; }
        
        public WithdrawalTransaction(int inputWithdraw)
        {
            InputWithdraw = inputWithdraw;

            CashSetList = new List<CashSet>();
        }

        public int GetTransactAmount()
        {
            int transactAmount = 0;

            foreach (CashSet cashSet in CashSetList)
            {
                transactAmount += cashSet.CashAmount;
            }

            return transactAmount;
        }
        
    }

    public class CashSet
    {
        public eNote Note { get; set; }
        public int NoteNum { get; set; }

        public CashSet(eNote note, int noteNum)
        {
            Note = note;
            NoteNum = noteNum;
        }

        public override string ToString()
        {
            return String.Format("£{0}x{1},", (int)Note, NoteNum );
        }

        public int CashAmount
        {
            get
            {
                return NoteNum * (int)Note;
            }
        }
    }

    //ALGORITHM_1
    public class TransactionModeAlgoA : ITransaction
    {        
        int withdrawSetTotalAmount = 0;

        public WithdrawalTransaction CreateTransaction(List<CashDeck> cashDeckList, int inputWithdraw)
        {
            WithdrawalTransaction wt = new WithdrawalTransaction(inputWithdraw);

            int requestedCashAmount = inputWithdraw - withdrawSetTotalAmount;

            for (int i = 0; i < cashDeckList.Count(); i++)
            {
                CashSet requestedCashSet = cashDeckList[i].AskMaxCashAmount(requestedCashAmount - withdrawSetTotalAmount);

                int setCashAmount = (int)requestedCashSet.Note * requestedCashSet.NoteNum;

                if (setCashAmount != 0)
                {
                    wt.CashSetList.Add(requestedCashSet);
                    withdrawSetTotalAmount += setCashAmount;
                }

                if (withdrawSetTotalAmount == requestedCashAmount)
                {                   
                    break;
                }                
            }
            return wt;
        }
    }

    public class TransactionModeAlgoB : ITransaction
    {        
        public WithdrawalTransaction CreateTransaction(List<CashDeck> cashDeckList, int inputWithdraw)
        {           
            return null; // to be implemented
        }
    }

    public enum eNote
    {
        GBP1 = 1, 
        GBP2 = 2, 
        GBP5 = 5, 
        GBP10 = 10, 
        GBP20 = 20,
        GBP50 = 50
    }

    public enum eTransactionMode
    {
        None,
        ALGORITHM_1,
        ALGORITHM_2
    }
}
