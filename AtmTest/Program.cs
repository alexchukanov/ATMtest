using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtmTest
{
    public class Program
    {
        static eTransactionMode transactionMode = eTransactionMode.Algo1;
        static int withdrawlInput = 100;

        static Atm atm = null;

        static void Main(string[] args)
        {
            List<CashDeck> cashDeckList = new List<CashDeck>
            {
               // 100x£1, 100x£2, 50x£5, 50x£10, 50x£20, 50x£50.
                new CashDeck(eNote.GBP50,50),
                new CashDeck(eNote.GBP20,50),
                new CashDeck(eNote.GBP10,50),
                new CashDeck(eNote.GBP5, 50),
                new CashDeck(eNote.GBP2, 100),
                new CashDeck(eNote.GBP1, 100),
            };

            switch(transactionMode)
            {
                case eTransactionMode.Algo1:
                    atm = new Atm(cashDeckList, new TransactionModeAlgo1());
                    break;

                case eTransactionMode.Algo2:
                    atm = new Atm(cashDeckList, new TransactionModeAlgo2());
                    break;

                default:
                    break;
            }

            double startBalance = atm.GetBalance();

            if (startBalance < withdrawlInput)
            {
                Atm.PrintMessage("Sorry, ATM is empty");
                return;
            }

            if (atm.WithdrawMoney(withdrawlInput))
            {
                Atm.PrintMessage("Transaction is OK, take your receipt and money");
                
            }
            else
            {
                Atm.PrintMessage("Transaction is failed, take your card");
            }
        }
    }

    public interface ITransaction
    {
        WithdrawTransaction CreateTransaction(List<CashDeck> cashDeckList, int entry);
    }

    public class Atm
    {
        ITransaction transactionModeAlgo = null;
        List<CashDeck> cashDeckList = null;
        WithdrawTransaction withdrawTransaction = null;

        public Atm(List<CashDeck> cashDeckList, ITransaction transactionModeAlgo )
        {
            this.cashDeckList = cashDeckList;
            this.transactionModeAlgo = transactionModeAlgo;
        }

        public bool WithdrawMoney(int entry)
        {
            withdrawTransaction = transactionModeAlgo.CreateTransaction(cashDeckList, entry);

            int transactionAmount = withdrawTransaction.GetTransactionAmount();
            int atmBalance = GetBalance();

            if(transactionAmount < atmBalance)
            {
                PrintMessage("Transaction is failed, choose anover amount");
                return false;
            }

            return PostTransaction();
        }

        public int GetAtmBalance()
        {
            return 0;
        }

        public static void PrintMessage(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
        }

        public bool PostTransaction()
        {
            CashDeck [] cashDeckListCopy = null;
            cashDeckList.CopyTo(cashDeckListCopy);

            foreach (CashDeck cashDeck in cashDeckListCopy)
            {
                if(cashDeck.RequestedNoteNum != 0 && cashDeck.RequestedNoteNum <= cashDeck.NoteNum)
                {
                    cashDeck.NoteNum -= cashDeck.RequestedNoteNum;
                    cashDeck.RequestedNoteNum = 0;
                }
                else
                {
                    PrintMessage("Transaction is failed, take your card");
                    return false;
                }
            }

            cashDeckList = new List<CashDeck>(cashDeckListCopy);

            return true;
        }

        public void PrintReceipt()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format(@"ALGORITHM 1: {0}", transactionModeAlgo));
            sb.AppendLine("Input [Withdrawal amount]");
            sb.AppendLine(withdrawTransaction.InputWithdraw.ToString());
            sb.AppendLine("Output");

            foreach (CashSet cashSet in withdrawTransaction.CashSetList)
            {
                sb.Append(String.Format(@"{0},",cashSet.ToString()) );
            }

            sb.Remove(sb.Length-1, 2);
            sb.AppendLine(String.Format(@"{0} balance, GBP", GetAtmBalance()));

            PrintMessage("");
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

        public int AskMaxCashAmount(int cashAmount)
        {
            int requestedNoteNum = 0;

            if (NoteNum != 0)
            {
                if (NoteNum == 1 && (int)Note < cashAmount)
                {
                    requestedNoteNum = 1;
                }
                else
                {
                    requestedNoteNum = cashAmount % (int)Note;
                }
            }

            RequestedNoteNum = requestedNoteNum;

            return requestedNoteNum * (int)Note;
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

    public class WithdrawTransaction
    {
        public List<CashSet> CashSetList { get; set; }
        public int InputWithdraw { get; set; }

        public WithdrawTransaction(int inputWithdraw)
        {
            InputWithdraw = inputWithdraw;

            CashSetList = new List<CashSet>();

        }

        public int GetTransactionAmount()
        {
            return 0;
        }

        public string PrintReceipt()
        {
            return "";
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
            return "";
        }

        public int CashAmount
        {
            get
            {
                return NoteNum * (int)Note;
            }
        }
    }

    public class TransactionModeAlgo1 : ITransaction
    {        
        int calcAmount = 0;

        public WithdrawTransaction CreateTransaction(List<CashDeck> cashDeckList, int inputWithdraw)
        {
            WithdrawTransaction wt = new WithdrawTransaction(inputWithdraw);

            int requestedCashAmount = inputWithdraw - calcAmount;

            for (int i = 0; i < cashDeckList.Count(); i++)
            {
                int calcAmount = cashDeckList[i].AskMaxCashAmount(requestedCashAmount);

                if (calcAmount != 0)
                {
                    CashSet cashSet = new CashSet(cashDeckList[i].Note, cashDeckList[i].NoteNum);

                    wt.CashSetList.Add(cashSet);
                }

                if (calcAmount == requestedCashAmount)
                {
                    break;
                }                
            }

            return wt;
        }
    }

    public class TransactionModeAlgo2 : ITransaction
    {
        public WithdrawTransaction CreateTransaction(List<CashDeck> cashDeckList, int inputWithdrawl)
        {
            return null;
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

    enum eTransactionMode
    {
        NotDefine,
        Algo1,
        Algo2
    }
}
