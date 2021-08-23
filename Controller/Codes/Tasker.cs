using System.Linq;
using System.Threading;

namespace Controller.Codes
{
    public class Tasker
    {
        private static long TotalTasks = 0;
        public static ADVCollection<PendingTask> PendingTasks { get; } = new ADVCollection<PendingTask>(true);
        public static ADVCollection<CompletedTask> CompletedTasks { get; } = new ADVCollection<CompletedTask>(true);

        public static long EnqueueTask(string TskDet)
        {
            long TempID = TotalTasks++;
            var NTskr = new PendingTask(TempID, TskDet);
            PendingTasks.Add(NTskr);
            return TempID;
        }

        public static void UpdateTask(ActionClass.TaskReport TRep)
        {
            PendingTask UpTarget = null;
            if (TRep.FutID >= 0) FutureUI.UpdateFuture(TRep.FutID, TRep.IsFutSucess);
            var SearchAr = PendingTasks.ToArray().Where(Tsk => Tsk.ID == TRep.TaskID).ToArray();
            if (SearchAr != null && SearchAr.Count() > 0) UpTarget = SearchAr[0]; else return;

            if (TRep.HasFinished)
            {
                var UPStr = TRep.IsFailed ? "Failed" : "Succeed";
                UpTarget.UpdateStatus(UPStr);
                
                Thread.Sleep(2000);
                PendingTasks.Remove(UpTarget);
                CompletedTasks.Add(new CompletedTask(UpTarget, TRep.Error));
            }
            else UpTarget.UpdateStatus(TRep.Status);
        }
    }
}
