# CSV 기반 타일맵 자동 배치 시스템

## 1. 기능 개요

엑셀에서 작성한 타일 배치 데이터를 CSV 파일로 저장하고, Unity에서 해당
파일을 읽어 지정된 위치에 타일 오브젝트를 자동으로 생성하는 시스템이다.

기존에는 Unity 에디터에서 타일 오브젝트를 직접 배치해야 했지만, CSV
파일에 숫자로 타일 정보를 작성하면 해당 데이터에 맞춰 스테이지를 구성할
수 있도록 구현하였다.

CSV에 작성된 숫자는 타일 ID와 연결되며, 실제 타일 오브젝트 생성은 기존
`TileMapSaveLoad`와 `TileMapTileDefinition` 구조를 활용한다.

**주요 목표** - CSV 파일을 통한 스테이지 배치 데이터 관리 - 숫자 코드와
타일 ID 간의 매핑 - CSV의 행과 열을 Unity 셀 좌표로 변환 - 기존
`TileMapSaveLoad`, `TileMapData`, `TileMapTileDefinition` 구조와 연동 -
생성된 타일을 정리하는 기능 제공

## 2. AI 활용 및 입력 프롬프트

**활용 도구:** ChatGPT

**활용 목적:** CSV 파일을 활용한 타일맵 자동 배치 시스템의 설계 및 기존
저장·로드 시스템과의 연동

### 최초 입력 프롬프트

> 내가 오브젝트를 파일로 읽어 와서 배치하는 시스템 만들고 싶어.
>
> 지금 타일 아이디는 Platform, Bridge, Ground가 있어.
>
> 그래서 예를 들어 그라운드는 1, 브리지는 2, 플랫폼은 3으로 해서
> 엑셀파일에 배치된 숫자들로 똑같이 배치하는 시스템 만들고 싶어.

### 추가 질문 및 수정 과정

최초 요청 이후 기존 프로젝트의 `CSVReader` 코드를 AI에게 제공하고, 기존
코드와 새롭게 구현할 타일맵 로더의 연동 방법을 확인하였다.

-   기존 `CSVReader`의 데이터 파싱 방식 분석
-   헤더가 없는 타일맵 데이터에 기존 `CSVReader`를 사용할 수 있는지 검토
-   기존 `TileMapSaveLoad` 및 `TileMapData` 구조와 연동하는 방법 확인
-   CSV의 숫자를 타일 ID로 변환하는 매핑 방식 적용
-   엑셀의 행과 열을 Unity의 셀 좌표로 변환하는 방식 적용
-   기존 `TileMapTileDefinition` 에셋을 활용하도록 타일 ID 연결 구조
    개선
-   Inspector에서 숫자 코드와 타일 ID의 매핑을 설정할 수 있도록 수정

### AI 활용 결과

최초 요청을 바탕으로 CSV 데이터를 읽어 타일을 배치하는 기본 구조를
제안받았다.

이후 기존 CSVReader의 구조적 특징을 확인하고, 타일맵 데이터에 적합한
별도의 그리드 전용 CSV 로더를 구성하였다.

추가 수정 과정에서는 타일 프리팹을 로더에서 직접 관리하는 대신, 기존
`TileMapSaveLoad`가 참조하는 `TileMapTileDefinition` 에셋을 활용하도록
연결 구조를 개선하였다.

## 3. 타일 코드 매핑 규칙

CSV에 작성된 숫자를 실제 타일 ID로 변환하기 위해 다음과 같은 규칙을
사용한다.

  숫자 코드   타일 ID    설명
  ----------- ---------- ------------------
  0           없음       빈 공간으로 처리
  1           Ground     지면 타일
  2           Bridge     다리 타일
  3           Platform   플랫폼 타일

숫자 코드와 타일 ID의 연결은 `TileMapGridLoader`의 `codeMappings`
목록에서 관리한다.

따라서 새로운 타일 종류가 추가되더라도 코드 내부의 Dictionary를 직접
수정하지 않고 Inspector에서 숫자 코드와 타일 ID를 연결할 수 있다.

## 4. CSV 데이터 작성 방식

엑셀에서 타일 배치 정보를 숫자로 작성하고 CSV 형식으로 저장한다.

**CSV 데이터 예시**

``` csv
0,1,1,1,0
0,2,2,2,0
3,3,0,3,3
```

위 데이터는 다음과 같은 규칙으로 해석된다.

-   `0`: 오브젝트를 생성하지 않는 빈 공간
-   `1`: Ground 타일 생성
-   `2`: Bridge 타일 생성
-   `3`: Platform 타일 생성

CSV 파일은 `Assets/Data/CSV` 폴더에 배치하며, Inspector의
`csvFileName`에는 확장자를 제외한 파일명을 입력한다.

## 5. 기존 CSVReader 분석 및 개선

### 기존 CSVReader의 구조

프로젝트에는 이미 CSV 파일을 읽어 데이터를 처리하는 `CSVReader.cs`가
존재하였다.

기존 CSVReader는 첫 번째 행을 헤더로 인식하고, 이후 행의 데이터를 헤더
이름에 맞춰 Dictionary 형태로 저장하는 구조였다.

### 발견한 문제점

기존 CSVReader는 아이템 정보나 스탯처럼 첫 번째 행에 컬럼 이름이
존재하는 데이터 테이블을 처리하기 위한 구조였다.

하지만 타일맵 데이터는 별도의 헤더 없이 숫자가 행과 열 형태로 나열된다.

따라서 기존 CSVReader를 그대로 사용하면 첫 번째 행이 헤더로 처리되어
실제 타일 배치 데이터가 누락될 수 있었다.

### 개선 방향

기존 CSVReader를 수정하는 대신, 타일맵 데이터의 형식에 맞는 별도의
`TileMapGridLoader.cs`를 구성하였다.

이를 통해 기존 데이터 테이블 처리 기능은 유지하면서 타일맵 데이터는
독립적으로 읽고 처리할 수 있도록 분리하였다.

## 6. 주요 구현 내용

### 6-1. 숫자 코드와 타일 ID 매핑 목록

`TileCodeMapping` 구조체를 사용하여 CSV의 숫자 코드와 `TileDefinition`에
정의된 타일 ID를 연결한다.

``` csharp
[System.Serializable]
public struct TileCodeMapping
{
    public int tileCode;
    public string tileID;
}

[SerializeField] private List<TileCodeMapping> codeMappings =
    new List<TileCodeMapping>()
{
    new TileCodeMapping { tileCode = 1, tileID = "Ground" },
    new TileCodeMapping { tileCode = 2, tileID = "Bridge" },
    new TileCodeMapping { tileCode = 3, tileID = "Platform" }
};
```

기존에는 Dictionary에 숫자와 타일 ID를 코드 내부에 고정해 두었지만,
수정된 구조에서는 Inspector의 `codeMappings` 목록에서 매핑 정보를 관리할
수 있도록 개선하였다.

### 6-2. 매핑 캐시 생성

CSV를 읽기 전에 Inspector에 설정된 매핑 목록을 Dictionary로 변환한다.

``` csharp
private Dictionary<int, string> mappingCache =
    new Dictionary<int, string>();

private void BuildMappingCache()
{
    mappingCache.Clear();

    foreach (var mapping in codeMappings)
    {
        if (!mappingCache.ContainsKey(mapping.tileCode))
        {
            mappingCache.Add(mapping.tileCode, mapping.tileID);
        }
    }
}
```

Dictionary를 이용하면 CSV에서 읽은 숫자 코드에 해당하는 타일 ID를 조회할
수 있다.

현재 코드는 동일한 숫자 코드가 중복 등록된 경우 먼저 등록된 매핑을
유지한다.

### 6-3. CSV 파일 읽기

`LoadGridFromCSV()`에서 필요한 참조 객체가 연결되어 있는지 확인한 후
매핑 캐시를 생성하고 CSV 파일을 읽는다.

``` csharp
if (stageData == null || saveLoadScript == null)
{
    Debug.LogError("StageData 또는 TileMapSaveLoad가 연결되지 않았습니다.");
    return;
}

BuildMappingCache();

string path = Path.Combine(
    Application.dataPath,
    "Data/CSV",
    csvFileName + ".csv"
);

if (!File.Exists(path))
{
    Debug.LogError($"CSV 파일을 찾을 수 없습니다: {path}");
    return;
}

string[] lines = File.ReadAllLines(path);
stageData.tiles.Clear();
```

파일의 각 행을 콤마 기준으로 분리하고, 각 값을 정수로 변환한다.

`0`은 빈 공간이므로 건너뛰며, 매핑되지 않은 숫자 코드는 경고 로그를
출력하고 해당 위치의 타일을 등록하지 않는다.

### 6-4. 좌표 변환

엑셀의 행과 열을 Unity에서 사용하는 셀 좌표로 변환한다.

``` csharp
int cellX = x;
int cellY = totalRows - 1 - y;
```

엑셀 데이터는 일반적으로 왼쪽 위에서부터 아래 방향으로 행이 증가한다.

현재 구현에서는 Unity의 Y축 배치 방향에 맞추기 위해 행 좌표를
반전시킨다.

### 6-5. TileData 등록

숫자 코드에서 찾은 타일 ID와 변환한 셀 좌표를 `TileData`로 만들어 기존
`TileMapData`에 등록한다.

``` csharp
stageData.tiles.Add(new TileData
{
    cellPosition = new Vector3Int(cellX, cellY, 0),
    tileID = tileID
});
```

CSV에서 읽어온 타일 정보를 기존 `TileMapData`의 `tiles` 목록에 추가한다.

이 단계에서는 실제 프리팹을 직접 생성하지 않고, 타일 배치에 필요한
데이터만 저장한다.

### 6-6. TileDefinition과 기존 배치 시스템 연동

모든 CSV 데이터 처리가 끝나면 기존 `TileMapSaveLoad`의
`LoadTileDataInEditor()`를 호출한다.

``` csharp
saveLoadScript.LoadTileDataInEditor();
```

`TileMapGridLoader`는 숫자 코드와 타일 ID를 연결하는 역할을 담당하고,
실제 타일 오브젝트 생성은 기존 `TileMapSaveLoad`에 맡긴다.

`TileMapSaveLoad`에 연결된 `TileMapTileDefinition` 에셋을 통해 해당 타일
ID에 맞는 타일 정의와 프리팹을 참조하여 배치하는 구조이다.

이를 통해 타일 프리팹 관리와 CSV 데이터 처리를 분리하고 기존 타일 배치
시스템을 재사용할 수 있도록 하였다.

### 6-7. 맵 초기화

`ClearMap()`을 호출하면 기존 `TileMapSaveLoad`의 `Clear3DTiles()`를
실행하여 생성된 타일을 정리한다.

``` csharp
public void ClearMap()
{
    if (saveLoadScript != null)
    {
        saveLoadScript.Clear3DTiles();
        Debug.Log("[TileMapGridLoader] 생성된 맵 타일 정리 완료");
    }
}
```

## 7. 전체 동작 과정

1.  엑셀에서 숫자를 사용하여 타일 배치 데이터를 작성한다.
2.  작성한 데이터를 CSV 파일로 저장한다.
3.  Unity의 `TileMapGridLoader`에서 CSV 파일명을 지정한다.
4.  Inspector의 `codeMappings`에서 숫자 코드와 타일 ID를 연결한다.
5.  `LoadGridFromCSV()`를 호출하여 CSV 파일을 읽는다.
6.  숫자 코드를 타일 ID로 변환하고 셀 좌표를 계산한다.
7.  변환한 정보를 기존 `TileMapData`에 등록한다.
8.  `TileMapSaveLoad.LoadTileDataInEditor()`를 호출한다.
9.  기존 `TileMapTileDefinition`에 등록된 타일 정의를 바탕으로 타일
    오브젝트를 배치한다.
10. 필요하면 `ClearMap()`으로 생성된 타일을 정리한다.

## 8. 구현 구조 및 역할 분리

  스크립트 / 에셋           역할
  ------------------------- --------------------------------------------
  `CSVReader.cs`            헤더가 있는 일반 데이터 테이블 파싱
  `TileMapGridLoader.cs`    타일맵 CSV 파싱, 숫자 코드 매핑, 좌표 변환
  `TileMapData.cs`          타일 배치 데이터 보관
  `TileMapSaveLoad.cs`      타일 데이터 로드 및 실제 오브젝트 생성
  `TileMapTileDefinition`   타일 ID와 타일 프리팹 등의 정의 정보 관리

각 구성 요소의 역할을 분리하여 CSV 파싱, 타일 데이터 관리, 실제 오브젝트
생성을 독립적으로 처리하도록 구성하였다.

## 9. 구현 결과 및 기대 효과

CSV 파일에 작성한 숫자를 기반으로 타일 배치 정보를 구성하고, 기존 타일
정의 에셋 및 저장·로드 시스템과 연동하는 구조를 구현하였다.

**주요 효과** - Unity 에디터에서 타일을 하나씩 배치하는 작업을 줄일 수
있다. - 엑셀을 활용하여 스테이지 배치 데이터를 직관적으로 관리할 수
있다. - 숫자 변경만으로 타일 종류와 배치 위치를 수정할 수 있다. -
Inspector에서 숫자 코드와 타일 ID의 매핑을 수정할 수 있다. - 기존
`TileMapTileDefinition`과 `TileMapSaveLoad`를 재사용하여 프리팹 관리
로직의 중복을 줄일 수 있다. - CSV 파싱과 실제 타일 생성의 역할을
분리하여 구조를 명확하게 구성할 수 있다. - `ClearMap()`을 통해 생성된
타일을 정리할 수 있다.

**관련 스크립트 및 에셋** - `CSVReader.cs`: 기존 데이터 테이블 CSV
파싱 - `TileMapGridLoader.cs`: 타일맵 전용 CSV 파싱 및 데이터 등록 -
`TileMapData.cs`: 타일 배치 데이터 관리 - `TileMapSaveLoad.cs`: 타일
데이터 로드 및 오브젝트 생성 - `TileMapTileDefinition`: 타일 ID와 프리팹
정의 정보 관리

## 10. 향후 개선 사항

-   CSV 파일의 행과 열이 일정하지 않을 경우에 대한 예외 처리
-   잘못된 숫자 및 데이터 형식에 대한 검증 강화
-   동일한 숫자 코드가 중복 등록되었을 때 경고를 출력하는 기능
-   다양한 스테이지 CSV 파일을 선택하여 불러오는 기능
-   타일 배치 완료 후 `TileColliderMerger`와 연동하여 충돌 영역 최적화
-   에디터에서 CSV 파일을 선택하고 즉시 배치 결과를 확인할 수 있는 기능
-   빌드 환경에서도 CSV 파일을 읽을 수 있도록 파일 경로 및 배포 방식
    개선
