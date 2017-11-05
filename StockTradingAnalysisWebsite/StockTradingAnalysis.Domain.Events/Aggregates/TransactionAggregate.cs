﻿using StockTradingAnalysis.Core.Common;
using StockTradingAnalysis.Domain.Events.Events;
using StockTradingAnalysis.Interfaces.Domain;
using StockTradingAnalysis.Interfaces.Events;
using StockTradingAnalysis.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockTradingAnalysis.Domain.Events.Aggregates
{
    public class TransactionAggregate : AggregateRoot,
        IHandle<TransactionBuyingOrderAddedEvent>,
        IHandle<TransactionSellingOrderAddedEvent>,
        IHandle<TransactionUndoEvent>,
        IHandle<TransactionPerformanceCalculatedEvent>,
        IHandle<TransactionDividendOrderAddedEvent>,
        IHandle<TransactionDividendCalculatedEvent>,
        IHandle<TransactionSplitOrderAddedEvent>,
        IHandle<StockOverallPerformanceChangedEvent>
    {
        /// <summary>
        /// Gets the aggregate id
        /// </summary>
        public override Guid Id { get; protected set; }

        //Shared        
        private DateTime OrderDate { get; set; }
        private decimal Units { get; set; }
        private decimal PricePerUnit { get; set; }
        private decimal OrderCosts { get; set; }
        private string Description { get; set; }
        private string Tag { get; set; }
        private IImage Image { get; set; }
        private Guid StockId { get; set; }
        private decimal PositionSize { get; set; }

        //Buying transaction
        private decimal InitialSL { get; set; }
        private decimal InitialTP { get; set; }
        private Guid StrategyId { get; set; }

        //Selling transaction
        private decimal Taxes { get; set; }
        private decimal? MAE { get; set; }
        private decimal? MFE { get; set; }
        private IEnumerable<Guid> Feedback { get; set; }

        /// <summary>
        /// Initializes a buying(opening) transaction
        /// </summary>
        /// <param name="stockId">The undertying stock</param>
        /// <param name="strategyId">The strategy used for this trade</param>
        /// <param name="id">The id of the transaction</param>
        /// <param name="orderDate">Order date</param>
        /// <param name="shares">Amount of shares</param>
        /// <param name="pricePerShare">Price per share</param>
        /// <param name="orderCosts">Order costs</param>
        /// <param name="description">Description</param>
        /// <param name="tag"></param>
        /// <param name="image"></param>
        /// <param name="initialSL"></param>
        /// <param name="initialTP"></param>
        public void CreateBuyingTransaction(
            Guid id,
            DateTime orderDate,
            decimal shares,
            decimal pricePerShare,
            decimal orderCosts,
            string description,
            string tag,
            IImage image,
            decimal initialSL,
            decimal initialTP,
            Guid stockId,
            Guid strategyId)
        {
            Id = id;

            var service = DependencyResolver.GetService<ITransactionPerformanceService>();

            var positionSize = (pricePerShare * shares) + orderCosts;
            var crv = service.GetCRV(initialTP, initialSL, pricePerShare, orderCosts, shares);

            ApplyChange(new TransactionBuyingOrderAddedEvent(id, typeof(TransactionAggregate), orderDate, shares,
                pricePerShare, orderCosts, description, tag, image, initialSL, initialTP, stockId, strategyId, positionSize, crv));
        }

        /// <summary>
        /// Initializes a selling(closing) transaction
        /// </summary>
        /// <param name="id">The id of this transaction</param>
        /// <param name="orderDate">Order date</param>
        /// <param name="shares">Amount of shares</param>
        /// <param name="pricePerShare">Price per share</param>
        /// <param name="orderCosts">Order costs</param>
        /// <param name="description">Description</param>
        /// <param name="tag">Tag</param>
        /// <param name="image">Image</param>
        /// <param name="stockId">The underlying stock</param>
        /// <param name="taxes">Taxes</param>
        /// <param name="mae">MAE</param>
        /// <param name="mfe">MFE</param>
        /// <param name="feedback">Feedback</param>
        public void CreateSellingTransaction(
            Guid id,
            DateTime orderDate,
            decimal shares,
            decimal pricePerShare,
            decimal orderCosts,
            string description,
            string tag,
            IImage image,
            Guid stockId,
            decimal taxes,
            decimal? mae,
            decimal? mfe,
            IEnumerable<Guid> feedback)
        {
            Id = id;

            var positionSize = (pricePerShare * shares) + orderCosts;

            ApplyChange(new TransactionSellingOrderAddedEvent(id, typeof(TransactionAggregate), orderDate, shares, pricePerShare, orderCosts, description,
                tag, image, stockId, taxes, mae, mfe, feedback, positionSize));
        }

        /// <summary>
        /// Initializes a dividend transaction
        /// </summary>
        /// <param name="id">The id of this transaction</param>
        /// <param name="orderDate">Order date</param>
        /// <param name="shares">Amount of shares</param>
        /// <param name="pricePerShare">Price per share</param>
        /// <param name="orderCosts">Order costs</param>
        /// <param name="description">Description</param>
        /// <param name="tag">Tag</param>
        /// <param name="image">Image</param>
        /// <param name="stockId">The underlying stock</param>
        /// <param name="taxes">Taxes</param>
        public void CreateDividendTransaction(
            Guid id,
            DateTime orderDate,
            decimal shares,
            decimal pricePerShare,
            decimal orderCosts,
            string description,
            string tag,
            IImage image,
            Guid stockId,
            decimal taxes)
        {
            Id = id;

            var positionSize = (pricePerShare * shares) + orderCosts;

            ApplyChange(new TransactionDividendOrderAddedEvent(id, typeof(TransactionAggregate), orderDate, shares, pricePerShare, orderCosts, description,
                tag, image, stockId, taxes, positionSize));
        }

        /// <summary>
        /// Initializes a split/reverse split transaction
        /// </summary>
        /// <param name="id">The id of this transaction</param>
        /// <param name="orderDate">Order date</param>
        /// <param name="shares">Amount of shares</param>
        /// <param name="pricePerShare">Price per share</param>       
        /// <param name="stockId">The underlying stock</param>
        public void CreateSplitTransaction(
            Guid id,
            DateTime orderDate,
            decimal shares,
            decimal pricePerShare,
            Guid stockId)
        {
            Id = id;

            ApplyChange(new TransactionSplitOrderAddedEvent(id, typeof(TransactionAggregate), orderDate, shares, pricePerShare, stockId));
        }

        /// <summary>
        /// Removes the stock
        /// </summary>
        public void Undo()
        {
            ApplyChange(new TransactionUndoEvent(Id, typeof(TransactionAggregate)));
        }

        /// <summary>
        /// Calculates the performance for this transaction if its a selling transaction
        /// </summary>
        public void CalculatePerformance()
        {
            var service = DependencyResolver.GetService<ITransactionPerformanceService>();
            var book = DependencyResolver.GetService<ITransactionBook>();

            var entries = book.GetLastCommittedChanges(StockId).ToList();
            var sell = entries.FirstOrDefault(e => e.TransactionId == Id) as ISellingTransactionBookEntry;
            var buys = entries.Where(e => e.TransactionId != Id).Cast<IBuyingTransactionBookEntry>();

            var performance = service.GetPerformance(sell, buys, MFE, MAE);
            ApplyChange(new TransactionPerformanceCalculatedEvent(Id, typeof(TransactionAggregate),
                performance.ProfitAbsolute,
                performance.ProfitPercentage,
                performance.ProfitMade,
                performance.HoldingPeriod,
                performance.R,
                performance.ExitEfficiency,
                performance.EntryEfficiency,
                performance.MAEAbsolute,
                performance.MFEAbsolute));

            ApplyChange(new StockOverallPerformanceChangedEvent(StockId, typeof(TransactionAggregate), performance.ProfitAbsolute));
        }

        /// <summary>
        /// Calculates the performance for this transaction if its a dividend transaction
        /// </summary>
        public void CalculateDividendPerformance()
        {
            var service = DependencyResolver.GetService<ITransactionPerformanceService>();
            var book = DependencyResolver.GetService<ITransactionBook>();

            var entries = book.GetLastCommittedChanges(StockId).ToList();
            var dividend = entries.FirstOrDefault(e => e.TransactionId == Id) as IDividendTransactionBookEntry;
            var buys = entries.Where(e => e.TransactionId != Id).Cast<IBuyingTransactionBookEntry>();

            var performance = service.GetPerformance(dividend, buys);
            ApplyChange(new TransactionDividendCalculatedEvent(Id, typeof(TransactionAggregate),
                performance.ProfitAbsolute,
                performance.ProfitPercentage,
                performance.ProfitMade,
                performance.HoldingPeriod,
                performance.R));

            ApplyChange(new StockOverallPerformanceChangedEvent(StockId, typeof(TransactionAggregate), performance.ProfitAbsolute));
        }

        /// <summary>
        /// Handles the given event <paramref name="event"/>
        /// </summary>
        /// <param name="event">The event</param>
        public void Handle(TransactionBuyingOrderAddedEvent @event)
        {
            Id = @event.AggregateId;

            OrderDate = @event.OrderDate;
            Units = @event.Shares;
            PricePerUnit = @event.PricePerShare;
            OrderCosts = @event.OrderCosts;
            Description = @event.Description;
            Tag = @event.Tag;
            Image = @event.Image;
            InitialSL = @event.InitialSL;
            InitialTP = @event.InitialTP;
            StockId = @event.StockId;
            StrategyId = @event.StrategyId;
            PositionSize = @event.PositionSize;
        }

        /// <summary>
        /// Handles the given event <paramref name="event"/>
        /// </summary>
        /// <param name="event">The event</param>
        public void Handle(TransactionSellingOrderAddedEvent @event)
        {
            Id = @event.AggregateId;

            OrderDate = @event.OrderDate;
            Units = @event.Shares;
            PricePerUnit = @event.PricePerShare;
            OrderCosts = @event.OrderCosts;
            Description = @event.Description;
            Tag = @event.Tag;
            Image = @event.Image;
            StockId = @event.StockId;
            Taxes = @event.Taxes;
            MAE = @event.MAE;
            MFE = @event.MFE;
            Feedback = @event.Feedback;
            PositionSize = @event.PositionSize;
        }

        /// <summary>
        /// Handles the given event <paramref name="event"/>
        /// </summary>
        /// <param name="event">The event</param>
        public void Handle(TransactionDividendOrderAddedEvent @event)
        {
            Id = @event.AggregateId;

            OrderDate = @event.OrderDate;
            Units = @event.Shares;
            PricePerUnit = @event.PricePerShare;
            OrderCosts = @event.OrderCosts;
            Description = @event.Description;
            Tag = @event.Tag;
            Image = @event.Image;
            StockId = @event.StockId;
            Taxes = @event.Taxes;
            PositionSize = @event.PositionSize;
        }

        /// <summary>
        /// Handles the given event <paramref name="event"/>
        /// </summary>
        /// <param name="event">The event</param>
        public void Handle(TransactionUndoEvent @event)
        {
        }

        /// <summary>
        /// Handles the given event <paramref name="event"/>
        /// </summary>
        /// <param name="event">The event</param>
        public void Handle(TransactionPerformanceCalculatedEvent @event)
        {
        }

        /// <summary>
        /// Handles the given event <paramref name="event"/>
        /// </summary>
        /// <param name="event">The event</param>
        public void Handle(TransactionDividendCalculatedEvent @event)
        {
        }

        /// <summary>
        /// Handles the given event <paramref name="event"/>
        /// </summary>
        /// <param name="event">The event</param>
        public void Handle(StockOverallPerformanceChangedEvent @event)
        {
        }

        /// <summary>
        /// Handles the given event <paramref name="event"/>
        /// </summary>
        /// <param name="event">The event</param>
        public void Handle(TransactionSplitOrderAddedEvent @event)
        {
        }
    }
}