using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FehlerhaftPayroll.Domain
{
    public interface IEntity<out TId>
    {
        TId Id { get; }
    }

    public interface IEntity : IEntity<int>
    {
    }

    public interface IAggregate : IEntity
    {
    }

    public interface IAggregate<out TId> : IEntity<TId>
    {
    }
}