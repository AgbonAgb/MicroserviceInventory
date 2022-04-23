using System.Threading.Tasks;
using MassTransit;
using Play.CataLog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IRepository<CatalogItem> _Repository;
        public CatalogItemCreatedConsumer(IRepository<CatalogItem> Repository)
        {
            _Repository = Repository;
        }
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var msg = context.Message;
            //check if the item has been consumed before
            var msgexisted = await _Repository.GetAsync(msg.ItemId);
            if (msgexisted != null)
            {
                return;
            }
            else
            {
                msgexisted = new CatalogItem
                {
                    Id = msg.ItemId,
                    Name = msg.Name,
                    Description = msg.Description,

                };

                await _Repository.CreateAsync(msgexisted);
            }

        }
    }

}