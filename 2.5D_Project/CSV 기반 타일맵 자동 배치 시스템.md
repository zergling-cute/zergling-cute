# CSV 기반 타일맵 자동 배치 시스템

## 1. 기능 개요

엑셀에서 작성한 타일 배치 데이터를 CSV 파일로 저장하고, Unity에서 해당 파일을 읽어 지정된 위치에 타일 오브젝트를 자동으로 생성하는 시스템이다.

기존에는 Unity 에디터에서 타일 오브젝트를 직접 배치해야 했지만, CSV 파일에 숫자로 타일 정보를 작성하면 해당 데이터에 맞춰 스테이지를 구성할 수 있도록 구현하였다.

**주요 목표**
- CSV 파일을 통한 스테이지 배치 데이터 관리
- 숫자와 타일 오브젝트 간의 ID 매핑
- CSV의 행과 열을 Unity 좌표로 변환
- 기존 `TileMapSaveLoad`, `TileMapData` 구조와 연동

## 2. AI 활용 및 입력 프롬프트

**활용 도구:** ChatGPT

**활용 목적:** CSV 파일을 활용한 타일맵 자동 배치 시스템의 설계 및 기존 저장·로드 시스템과의 연동

### 최초 입력 프롬프트

> 내가 오브젝트를 파일로 읽어 와서 배치하는 시스템 만들고 싶어.
>
> 지금 타일 아이디는 Platform, Bridge, Ground가 있어.
>
> 그래서 예를 들어 그라운드는 1, 브리지는 2, 플랫폼은 3으로 해서 엑셀파일에 배치된 숫자들로 똑같이 배치하는 시스템 만들고 싶어.

### 추가 질문 및 수정 과정

최초 요청 이후 기존 프로젝트의 CSVReader 코드를 AI에게 제공하고, 기존 코드와 새롭게 구현할 타일맵 로더의 연동 방법을 확인하였다.

- 기존 CSVReader의 데이터 파싱 방식 분석
- 헤더가 없는 타일맵 데이터에 기존 CSVReader를 사용할 수 있는지 검토
- 기존 `TileMapSaveLoad` 및 `TileMapData` 구조와 연동하는 방법 확인
- CSV의 숫자를 타일 ID로 변환하는 매핑 방식 적용
- 엑셀의 행과 열을 Unity의 셀 좌표로 변환하는 방식 적용

### AI 활용 결과

최초 요청을 바탕으로 CSV 데이터를 읽고 타일을 배치하는 기본 구조를 제안받았다.

이후 기존 CSVReader의 구조적 특징을 확인하고, 타일맵 데이터에 적합한 별도의 그리드 전용 CSV 로더를 사용하는 방향으로 구현을 발전시켰다.

## 3. 타일 ID 매핑 규칙

CSV에 작성된 숫자를 실제 타일 오브젝트의 ID로 변환하기 위해 다음과 같은 규칙을 설정하였다.

| 숫자 ID | 타일 ID | 설명 |
|---|---|---|
| 0 | 없음 | 빈 공간 |
| 1 | Ground | 지면 타일 |
| 2 | Bridge | 다리 타일 |
| 3 | Platform | 플랫폼 타일 |

숫자와 타일 ID를 매핑하여 CSV 데이터만으로 서로 다른 종류의 타일을 구분할 수 있도록 설계하였다.

## 4. CSV 데이터 작성 방식

엑셀에서 타일 배치 정보를 숫자로 작성하고 CSV 형식으로 저장한다.

**CSV 데이터 예시**

```csv
0,1,1,1,0
0,2,2,2,0
3,3,0,3,3
```

위 데이터는 다음과 같은 규칙으로 해석된다.

- `0`: 오브젝트를 생성하지 않는 빈 공간
- `1`: Ground 타일 생성
- `2`: Bridge 타일 생성
- `3`: Platform 타일 생성

이처럼 숫자만으로 스테이지의 형태와 타일 종류를 표현할 수 있다.

## 5. 기존 CSVReader 분석 및 개선

### 기존 CSVReader의 구조

프로젝트에는 이미 CSV 파일을 읽어 데이터를 처리하는 `CSVReader.cs`가 존재하였다.

기존 CSVReader는 첫 번째 행을 헤더로 인식하고, 이후 행의 데이터를 헤더 이름에 맞춰 Dictionary 형태로 저장하는 구조였다.

```csharp
var header = Regex.Split(lines[0], SPLIT_RE);

for (var i = 1; i < lines.Length; i++)
{
    var values = Regex.Split(lines[i], SPLIT_RE);

    var entry = new Dictionary<string, object>();

    for (var j = 0; j < header.Length && j < values.Length; j++)
    {
        entry[header[j]] = values[j];
    }

    list.Add(entry);
}
```

### 발견한 문제점

기존 CSVReader는 아이템 정보나 스탯처럼 첫 번째 행에 컬럼 이름이 존재하는 데이터 테이블을 처리하기 위한 구조였다.

하지만 타일맵 데이터는 별도의 헤더 없이 숫자가 행과 열 형태로 나열된다.

따라서 기존 CSVReader를 그대로 사용하면 첫 번째 행이 헤더로 처리되어 실제 타일 배치 데이터가 누락되는 문제가 발생할 수 있었다.

### 개선 방향

기존 CSVReader를 수정하는 대신, 타일맵 데이터의 형식에 맞는 별도의 `TileMapGridLoader.cs`를 구성하는 방향을 선택하였다.

이를 통해 기존 데이터 테이블 처리 기능은 유지하면서 타일맵 데이터는 독립적으로 읽고 처리할 수 있도록 분리하였다.

## 6. 주요 구현 내용

### 6-1. CSV 파일 읽기

CSV 파일의 전체 행을 읽고, 각 행을 콤마 기준으로 분리하여 타일 데이터를 추출한다.

```csharp
string[] lines = File.ReadAllLines(path);

for (int y = 0; y < lines.Length; y++)
{
    if (string.IsNullOrWhiteSpace(lines[y]))
        continue;

    string[] values = lines[y].Split(',');

    for (int x = 0; x < values.Length; x++)
    {
        if (!int.TryParse(values[x].Trim(), out int tileCode))
            continue;

        // 타일 데이터 처리
    }
}
```

각 행과 열을 순회하면서 숫자로 변환할 수 있는 값을 타일 코드로 사용한다.

### 6-2. 숫자와 타일 ID 매핑

Dictionary를 사용하여 숫자 코드와 실제 타일 ID를 연결하였다.

```csharp
private Dictionary<int, string> idMapping =
    new Dictionary<int, string>()
{
    { 1, "Ground" },
    { 2, "Bridge" },
    { 3, "Platform" }
};
```

CSV에서 읽어온 숫자를 Dictionary에서 검색하여 해당하는 타일 ID를 가져온다.

`0`은 빈 공간이므로 오브젝트를 생성하지 않으며, 정의되지 않은 숫자는 경고 로그를 출력하고 건너뛰도록 처리하였다.

### 6-3. 좌표 변환

엑셀의 행과 열을 Unity에서 사용하는 셀 좌표로 변환하였다.

```csharp
int cellX = x;
int cellY = totalRows - 1 - y;
```

엑셀 데이터는 일반적으로 왼쪽 위에서부터 아래 방향으로 행이 증가한다.

반면 Unity에서 위쪽을 양의 Y 방향으로 사용하는 배치 구조에 맞추기 위해 Y 좌표를 반전시켰다.

이를 통해 CSV에 작성된 배치 순서와 Unity에서 생성되는 타일의 위치를 대응시킬 수 있도록 하였다.

### 6-4. TileData 등록 및 오브젝트 생성

변환한 좌표와 타일 ID를 기존 `TileData` 구조에 저장하였다.

```csharp
stageData.tiles.Add(new TileData
{
    cellPosition = new Vector3Int(cellX, cellY, 0),
    tileID = tileID
});
```

CSV에서 읽어온 타일 정보를 기존 `TileMapData`의 `tiles` 목록에 추가한다.

모든 데이터 처리가 끝나면 기존 `TileMapSaveLoad`의 `LoadTileData()`를 호출하여 등록된 데이터를 기반으로 타일 오브젝트를 생성하도록 연동하였다.

```csharp
saveLoadScript.LoadTileData();
```

이를 통해 CSV 데이터 파싱과 실제 타일 오브젝트 생성 기능을 분리하고, 기존 타일맵 저장 및 로드 시스템을 재사용할 수 있도록 구성하였다.

## 7. 전체 동작 과정

1. 엑셀에서 숫자를 사용하여 타일 배치 데이터를 작성한다.
2. 작성한 데이터를 CSV 파일로 저장한다.
3. Unity에서 CSV 파일을 읽어온다.
4. 행과 열을 순회하며 숫자 데이터를 추출한다.
5. 숫자를 타일 ID로 변환한다.
6. CSV 좌표를 Unity 셀 좌표로 변환한다.
7. 변환한 정보를 `TileMapData`에 등록한다.
8. `TileMapSaveLoad`를 호출하여 타일 오브젝트를 생성한다.

## 8. 구현 결과 및 기대 효과

CSV 파일에 작성한 숫자를 기반으로 타일 배치 데이터를 관리하고, 기존 타일맵 시스템과 연동하여 오브젝트를 생성하는 구조를 설계하였다.

**주요 기대 효과**

- Unity 에디터에서 타일을 하나씩 배치하는 작업을 줄일 수 있다.
- 엑셀을 활용하여 스테이지 배치 데이터를 직관적으로 관리할 수 있다.
- 숫자 변경만으로 타일 종류와 배치 위치를 수정할 수 있다.
- 기존 CSVReader와 타일맵 로더의 역할을 분리하여 데이터 처리 구조를 명확하게 구성할 수 있다.
- 새로운 타일 ID를 추가하는 방식으로 확장할 수 있다.

**관련 스크립트**
- `CSVReader.cs`: 기존 데이터 테이블 CSV 파싱
- `TileMapGridLoader.cs`: 타일맵 전용 CSV 파싱 및 데이터 등록
- `TileMapData.cs`: 타일 배치 데이터 관리
- `TileMapSaveLoad.cs`: 타일 데이터 저장 및 오브젝트 생성

## 9. 향후 개선 사항

- CSV 파일의 행과 열이 일정하지 않을 경우에 대한 예외 처리
- 잘못된 숫자 및 데이터 형식에 대한 검증 강화
- 다양한 스테이지 CSV 파일을 선택하여 불러오는 기능
- 타일 배치 완료 후 `TileColliderMerger`와 연동하여 충돌 영역 최적화
- 에디터에서 CSV 파일을 선택하고 즉시 배치 결과를 확인할 수 있는 기능
