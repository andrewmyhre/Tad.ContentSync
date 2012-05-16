namespace Tad.ContentSync.Services
{
    public interface ISynchronisationJobRunner
    {
        void Process(ISynchronisationJobBuilder syncJobBuilder);
    }
}