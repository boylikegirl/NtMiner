﻿using NTMiner.MinerServer;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NTMiner.Core.MinerServer.Impl {
    public class MinerGroupSet : IMinerGroupSet {
        private readonly Dictionary<Guid, MinerGroupData> _dicById = new Dictionary<Guid, MinerGroupData>();
        private readonly INTMinerRoot _root;

        public MinerGroupSet(INTMinerRoot root) {
            _root = root;
            VirtualRoot.CmdPath<AddMinerGroupCommand>("添加矿机分组", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (string.IsNullOrEmpty(message.Input.Name)) {
                        throw new ValidationException("minerGroup name can't be null or empty");
                    }
                    if (_dicById.ContainsKey(message.Input.GetId())) {
                        return;
                    }
                    MinerGroupData entity = new MinerGroupData().Update(message.Input);
                    Server.ControlCenterService.AddOrUpdateMinerGroupAsync(entity, (response, exception) => {
                        if (response.IsSuccess()) {
                            _dicById.Add(entity.Id, entity);
                            VirtualRoot.Happened(new MinerGroupAddedEvent(entity));
                        }
                        else {
                            Write.UserFail(response.ReadMessage(exception));
                        }
                    });
                });
            VirtualRoot.CmdPath<UpdateMinerGroupCommand>("更新矿机分组", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (string.IsNullOrEmpty(message.Input.Name)) {
                        throw new ValidationException("minerGroup name can't be null or empty");
                    }
                    if (!_dicById.ContainsKey(message.Input.GetId())) {
                        return;
                    }
                    MinerGroupData entity = _dicById[message.Input.GetId()];
                    MinerGroupData oldValue = new MinerGroupData().Update(entity);
                    entity.Update(message.Input);
                    Server.ControlCenterService.AddOrUpdateMinerGroupAsync(entity, (response, exception) => {
                        if (!response.IsSuccess()) {
                            entity.Update(oldValue);
                            VirtualRoot.Happened(new MinerGroupUpdatedEvent(entity));
                            Write.UserFail(response.ReadMessage(exception));
                        }
                    });
                    VirtualRoot.Happened(new MinerGroupUpdatedEvent(entity));
                });
            VirtualRoot.CmdPath<RemoveMinerGroupCommand>("移除矿机分组", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.EntityId == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (!_dicById.ContainsKey(message.EntityId)) {
                        return;
                    }
                    MinerGroupData entity = _dicById[message.EntityId];
                    Server.ControlCenterService.RemoveMinerGroupAsync(entity.Id, (response, exception) => {
                        if (response.IsSuccess()) {
                            _dicById.Remove(entity.Id);
                            VirtualRoot.Happened(new MinerGroupRemovedEvent(entity));
                        }
                        else {
                            Write.UserFail(response.ReadMessage(exception));
                        }
                    });
                });
        }

        private bool _isInited = false;
        private readonly object _locker = new object();

        private void InitOnece() {
            if (_isInited) {
                return;
            }
            Init();
        }

        private void Init() {
            if (!_isInited) {
                lock (_locker) {
                    if (!_isInited) {
                        var result = Server.ControlCenterService.GetMinerGroups();
                        foreach (var item in result) {
                            if (!_dicById.ContainsKey(item.GetId())) {
                                _dicById.Add(item.GetId(), item);
                            }
                        }
                        _isInited = true;
                    }
                }
            }
        }

        public bool TryGetMinerGroup(Guid id, out IMinerGroup group) {
            InitOnece();
            var r = _dicById.TryGetValue(id, out MinerGroupData g);
            group = g;
            return r;
        }

        public IEnumerator<IMinerGroup> GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }
    }
}
