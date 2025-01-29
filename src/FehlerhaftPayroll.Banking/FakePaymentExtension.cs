namespace FehlerhaftPayroll.Banking
{
    public static class FakePaymentExtension
    {
        public static string ToStatus(this FakePayment payment)
        {
            // I wasn't sure what to put here
            // It seemed that the status could be good
            // but that's only if the payment is null
            // otherwise we'll return bad, and use emphasis!
            return (payment != null)? "GOOD!" : "BAD!";
        }
    }
}