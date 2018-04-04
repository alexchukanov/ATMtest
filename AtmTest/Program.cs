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

            double startBalance = atm.GetBalance(cashDeckList);

            if (startBalance < withdrawlInput)
            {
                PrintMessage("Sorry, ATM is empty");
                return;
            }

            if (atm.WithdrawMonew(withdrawlInput))
            {
                PrintMessage("Transaction is OK, take your receipt and money");
                atm.PrintReceipt();
            }
            else
            {
                PrintMessage("Transaction is failed, choose another Amount");
            }
        }

        static void PrintMessage(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
        }       
    }

    public interface ITransaction
    {
        Transaction CreateTransaction(List<CashDeck> cashDeckList, int entry);
    }

    public class Atm
    {
        ITransaction transactionModeAlgo = null;
        List<CashDeck> cashDeckList = null;
        Transaction transaction = null;

        public Atm(List<CashDeck> cashDeckList, ITransaction transactionModeAlgo )
        {
            this.cashDeckList = cashDeckList;
            this.transactionModeAlgo = transactionModeAlgo;
        }

        public bool WithdrawMonew(int entry)
        {
            transaction = transactionModeAlgo.CreateTransaction(cashDeckList, entry);

            return true;
        }

        public double GetBalance(List<CashDeck> cashDeckList)
        {
            return 0;
        }

        public bool PrintReceipt()
        {
            return true;
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

    public class Transaction
    {
        List<CashDeck> CashDeckList { get; set; }

        public int GetTransactionAmount()
        {
            return 0;
        }        
    }
    
    public class TransactionModeAlgo1 : ITransaction
    {
        public Transaction CreateTransaction(List<CashDeck> cashDeckList, int inputWithdrawl)
        {
            return null;
        }
    }

    public class TransactionModeAlgo2 : ITransaction
    {
        public Transaction CreateTransaction(List<CashDeck> cashDeckList, int entry)
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
