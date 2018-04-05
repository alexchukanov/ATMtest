using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtmTest
{
    public class Program
    {
        static eTransactionMode transactionMode = eTransactionMode.ALGORITHM_1;
        static int withdrawInput = 0;

        static Atm atm = null;

        static void Main(string[] args)
        {
            Atm.PrintMessage("Input amount in £ to withdraw", false);
            withdrawInput = Atm.ReadWithdrawlAmount();

            if (withdrawInput == 0)
            {
                Atm.PrintMessage("Are you OK?", true);
                return;
            }           

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
                case eTransactionMode.ALGORITHM_1:
                    atm = new Atm(cashDeckList, new TransactionModeAlgo1());
                    break;

                case eTransactionMode.ALGORITHM_2:
                    atm = new Atm(cashDeckList, new TransactionModeAlgo2());
                    break;

                default:
                    break;
            }

            double startBalance = atm.GetAtmBalance();

            //header
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n");
            sb.AppendLine(String.Format(transactionMode.ToString()));
            sb.AppendLine(String.Format(@"£{0:0.00} ATM balance", startBalance));
           
            Atm.PrintMessage(sb.ToString(), false);

            if (startBalance < withdrawInput)
            {
                Atm.PrintMessage("Please, try to withdraw a lesser amount", true);
                return;
            }

            bool isTrransactionOk = atm.WithdrawMoney(withdrawInput);

            if (isTrransactionOk)
            {
                atm.PrintReceipt();
                Atm.PrintMessage("Transaction is OK, take your receipt and money", true);                
            }
            else
            {
                Atm.PrintMessage("Transaction is failed, take your card", true);
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

        public Atm(List<CashDeck> cashDeckList, ITransaction transactionModeAlgo )
        {
            this.cashDeckList = cashDeckList;
            this.transactionModeAlgo = transactionModeAlgo;
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

        public static int ReadWithdrawlAmount()
        {
            string amount = Console.ReadLine();

            int amountValue = 0;

            if (!int.TryParse(amount, out amountValue))
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

    public class TransactionModeAlgo1 : ITransaction
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

    public class TransactionModeAlgo2 : ITransaction
    {
        int withdrawSetTotalAmount = 0;

        public WithdrawalTransaction CreateTransaction(List<CashDeck> cashDeckList, int inputWithdraw)
        {
            WithdrawalTransaction wt = new WithdrawalTransaction(inputWithdraw);

            int requestedCashAmount = inputWithdraw - withdrawSetTotalAmount;

            var cashSet = cashDeckList.SingleOrDefault(x => x.Note == eNote.GBP20);

            if (cashSet != null)
            {
                cashDeckList.Remove(cashSet);
            }


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
        ALGORITHM_1,
        ALGORITHM_2
    }
}
