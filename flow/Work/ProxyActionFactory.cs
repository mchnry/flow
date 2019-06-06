using Mchnry.Flow.Configuration;
using System.Collections.Generic;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{

    internal class ProxyActionFactory<TModel>
    {

        private Dictionary<string, IAction<TModel>> actions = new Dictionary<string, IAction<TModel>>();

        public ProxyActionFactory(Configuration.Config configuration)
        {
            this.Configuration = configuration;
        }
        public Config Configuration { get; set; }

        public void AddAction(string id, IAction<TModel> action)
        {
            id = ConventionHelper.EnsureConvention(NamePrefixOptions.Action, id, this.Configuration.Convention);
            if (!this.actions.ContainsKey(id))
            {
                this.actions.Add(id, action);
            }
        }


        public IActionFactory proxy { get; set; }

        public IAction<TModel> getAction(WorkDefine.ActionDefinition def)
        {
            WorkDefine.ActionDefinition withoutConvention = new WorkDefine.ActionDefinition()
            {
                Id = ConventionHelper.RemoveConvention(def.Id, this.Configuration.Convention),
                Description = def.Description
            };
            IAction<TModel> toReturn = default(IAction<TModel>);


            if (!this.actions.ContainsKey(def.Id) && !this.actions.ContainsKey(withoutConvention.Id))
            {

                if (this.proxy != null)
                {
                    try
                    {
                        string searchId = ConventionHelper.RemoveConvention(def.Id, this.Configuration.Convention);
                        toReturn = this.proxy.GetAction<TModel>(withoutConvention);
                        if (toReturn == null)
                        {
                            toReturn = this.proxy.GetAction<TModel>(def);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw new LoadActionException(def.Id, ex);
                    }

                    if (default(IAction<TModel>) == toReturn)
                    {
                        throw new LoadActionException(def.Id);
                    }

                    this.actions.Add(def.Id, toReturn);
                }
            }
            else
            {
                if (this.actions.ContainsKey(withoutConvention.Id))
                {
                    toReturn = this.actions[withoutConvention.Id];
                }

                else
                {
                    toReturn = this.actions[def.Id];
                }
            }
            return toReturn;
        }
    }
}
