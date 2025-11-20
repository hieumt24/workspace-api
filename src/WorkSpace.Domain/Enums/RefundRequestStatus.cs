namespace WorkSpace.Domain.Enums
{
    public enum RefundRequestStatus
    {
        PendingOwnerApproval, 
        ApprovedByOwner,     
        ApprovedByTimeout,    
        Rejected,           
        Processing,           
        Completed,            
        Failed               
    }
}