scenario {
    id;
    name;
    createdAt;
}

scenario_peds {
    id;
    scenarioId;

    model;
    outfitVariation?;
    position;
    rotation;

    isFreezed?;
    scenarioAnim?;
    anim?;
    animDict?;
    flags?;
    relationship?;
    
    weaponModel?;
}

scenario_vehicles {
    id;
    scenarioId;

    model;
    props;
    plate;
    position;
    rotation;

    pedDriver?;
    driverMetadata?;
}

scenario_props {
    id;
    scenarioId;

    model;
    position;
    rotation;

    attachedToPedId?;
    attachedMetadata?;
}

> Menu para selecionar uma entidade
> Através dessa entidade escolher o modo de
> Deletar, Editar e Atualizar.
> Salvar o Tipo e o ID da entidade no statebag


>> Scene Menu
>>> Create New Scene
>>> Select Scene
>>> Delete Scene

>> Scene Selected
>>> EDIT MODE bool
>>> Add new Entity
>>> Entity List
>>> Start Scene
>>> Stop Scene
>>> Restart Scene
>>> Save Scene

>> Entity Selected
>>> Edit Entity
>>> Delete Entity
>>> Reset Entity
>>> Rename Entity

>>> Edit Mode Enabled
>>>> Just look to entity and LEFT CLICK with mouse to Edit
>>>> And you select then just ENTER to confirm the new positio and rotation
>>>> RIGHT CLICK to cancel to release entity selected
