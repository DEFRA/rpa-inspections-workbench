using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        IGenericRepository<Breed> BreedRepository { get; }

        IGenericRepository<Cattle> CattleRepository { get; }

        IGenericRepository<Holding> HoldingRepository { get; }

        IGenericRepository<LinkedHolding> LinkedHoldingRepository { get; }

        IGenericRepository<PackRequested> PackRequestedRepository { get; }

        IGenericRepository<DataRequested> DataRequestedRepository { get; }

        void Commit();

    }
}    